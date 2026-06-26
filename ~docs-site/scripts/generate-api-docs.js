#!/usr/bin/env node

/**
 * 从 C# 源文件中提取 XML 文档注释，生成 Markdown API 文档供 Docusaurus 使用。
 * 
 * 用法: node scripts/generate-api-docs.js
 * 输出: docs/api/generated/
 */

const fs = require('fs');
const path = require('path');

const SOURCE_DIRS = [
  path.resolve(__dirname, '../../Runtime'),
  path.resolve(__dirname, '../../Editor'),
];
const OUTPUT_DIR = path.resolve(__dirname, '../docs/api/generated');
const SIDEBAR_FILE = path.resolve(__dirname, '../docs/api/generated-sidebar.json');

// ── 解析 XML 注释 ──────────────────────────────────────────────────────────

function extractXmlComments(text) {
  const result = { summary: '', typeParams: [], params: [], returns: '', remarks: '' };

  const summaryMatch = text.match(/<summary>([\s\S]*?)<\/summary>/);
  if (summaryMatch) {
    result.summary = summaryMatch[1]
      .replace(/\n\s*\/\/\/\s*/g, '\n')
      .replace(/^\s*\/\/\/\s*/, '')
      .trim();
  }

  const typeParamMatches = text.matchAll(/<typeparam\s+name="(\w+)">([\s\S]*?)<\/typeparam>/g);
  for (const m of typeParamMatches) {
    result.typeParams.push({ name: m[1], desc: m[2].trim() });
  }

  const paramMatches = text.matchAll(/<param\s+name="(\w+)">([\s\S]*?)<\/param>/g);
  for (const m of paramMatches) {
    result.params.push({ name: m[1], desc: m[2].trim() });
  }

  const returnsMatch = text.match(/<returns>([\s\S]*?)<\/returns>/);
  if (returnsMatch) {
    result.returns = returnsMatch[1].trim();
  }

  const remarksMatch = text.match(/<remarks>([\s\S]*?)<\/remarks>/);
  if (remarksMatch) {
    result.remarks = remarksMatch[1].trim();
  }

  return result;
}

// ── 解析 C# 方法签名 ──────────────────────────────────────────────────────

function parseMethodSignature(line, xmlComment) {
  // 匹配: [modifiers] returnType MethodName<T>(params)
  const methodSig = line.trim().replace(/\/\/\/.*$/, '').trim();
  const match = methodSig.match(
    /(?:public|private|protected|internal|static|virtual|override|abstract|sealed|async|unsafe|partial|extern|new\s+)*\s*(?:public|private|protected|internal|static|virtual|override|abstract|sealed|async|unsafe|partial|extern|new\s+)*\s*([\w<>\[\],.\s?]+)\s+(\w+)(<[\w\s,<>\[\]]+>)?\s*\(([\s\S]*)\)/
  );
  if (!match) return null;

  const returnType = match[1].trim();
  const name = match[2];
  const typeParams = match[3] || '';
  let params = match[4].trim();

  const parsedParams = [];
  if (params) {
    // 按逗号分割，注意泛型中的逗号
    const paramParts = splitParams(params);
    for (const part of paramParts) {
      const trimmed = part.trim();
      if (!trimmed) continue;

      // 匹配: [modifiers] type name [= default]
      const pMatch = trimmed.match(
        /(?:this\s+)?(?:ref\s+|out\s+|in\s+|params\s+)?([\w<>\[\],.\s?]+)\s+(\w+)(?:\s*=\s*[\s\S]*)?$/
      );
      if (pMatch) {
        const pDesc = xmlComment.params.find(p => p.name === pMatch[2]);
        parsedParams.push({
          type: pMatch[1].trim(),
          name: pMatch[2],
          desc: pDesc ? pDesc.desc : '',
        });
      } else {
        parsedParams.push({ type: '', name: trimmed, desc: '' });
      }
    }
  }

  return {
    name,
    returnType,
    typeParams,
    params: parsedParams,
    returns: xmlComment.returns,
  };
}

function splitParams(params) {
  const result = [];
  let depth = 0;
  let start = 0;
  for (let i = 0; i < params.length; i++) {
    if (params[i] === '<') depth++;
    else if (params[i] === '>') depth--;
    else if (params[i] === ',' && depth === 0) {
      result.push(params.substring(start, i));
      start = i + 1;
    }
  }
  result.push(params.substring(start));
  return result;
}

// ── 解析属性 ──────────────────────────────────────────────────────────────

function parseProperty(line, xmlComment) {
  const match = line.trim().match(
    /(?:public|private|protected|internal|static|virtual|override|abstract)\s+(?:[\w<>\[\],.\s?]+)\s+(\w+)\s*\{/
  );
  if (!match) return null;
  return { name: match[1], summary: xmlComment.summary };
}

// ── 解析单个 C# 文件 ──────────────────────────────────────────────────────

function parseCsFile(filePath) {
  const content = fs.readFileSync(filePath, 'utf-8');
  const lines = content.split('\n');

  // 提取命名空间
  let namespace = '';
  for (const line of lines) {
    const nsMatch = line.match(/^namespace\s+([\w.]+)/);
    if (nsMatch) {
      namespace = nsMatch[1];
      break;
    }
  }

  // 提取 using 语句来识别模块
  const fileLevelUsings = [];
  const modulePatterns = {
    'AssetManager': /AssetManager|Addressables|Resources/i,
    'MVC': /\bMVC\b|Controller|Model/i,
    'ObjectPool': /ObjectPool|Pool/i,
    'ReferencePool': /ReferencePool|IReference/i,
    'HFSM': /HFSM/i,
    'Procedure': /Procedure/i,
    'Core': /Core\b/i,
    'Serialize': /Serialize/i,
    'Sheet': /Sheet/i,
    'Utils': /Utils|HNDictionary/i,
  };

  let module = 'Core';
  for (const [mod, pattern] of Object.entries(modulePatterns)) {
    if (pattern.test(filePath)) {
      module = mod;
      break;
    }
  }

  // 提取所有 XML 注释块及其后的声明
  const members = [];
  let xmlCommentLines = [];
  let inXmlComment = false;

  for (let i = 0; i < lines.length; i++) {
    const line = lines[i].trim();

    if (line.startsWith('///')) {
      inXmlComment = true;
      xmlCommentLines.push(line);
      continue;
    }

    if (inXmlComment) {
      if (line === '' || line.startsWith('[') || line.startsWith('#')) {
        // 可能是空行或属性，继续等待声明
        if (line.startsWith('[')) {
          // 暂时忽略
        }
        if (line === '') continue;
      }

      // 解析声明
      const xmlComment = extractXmlComments(xmlCommentLines.join('\n'));

      if (line.startsWith('public ') || line.startsWith('internal ') || line.startsWith('protected ')) {
        if (line.includes('(') && !line.includes('=>') && !line.includes('{ get')) {
          const method = parseMethodSignature(line, xmlComment);
          if (method) {
            members.push({ type: 'method', ...method });
          }
        } else if (line.includes('{ get') || line.includes('{ set') || (line.includes('=>') && !line.includes('('))) {
          // 属性 - 需要更仔细的解析
          const prop = parseProperty(line, xmlComment);
          if (prop) {
            members.push({ type: 'property', ...prop });
          }
        } else if (
          matchDecl(line, 'class') || matchDecl(line, 'struct') || matchDecl(line, 'interface') || matchDecl(line, 'enum')
        ) {
          const declMatch = line.match(/(class|struct|interface|enum)\s+(\w+)/);
          if (declMatch) {
            return {
              filePath,
              namespace,
              module,
              kind: declMatch[1],
              name: declMatch[2],
              summary: xmlComment.summary,
              members,
            };
          }
        } else if (line.includes(' delegate ')) {
          const declMatch = line.match(/delegate\s+[\w<>\[\],.\s?]+\s+(\w+)\s*\(/);
          if (declMatch) {
            return {
              filePath,
              namespace,
              module,
              kind: 'delegate',
              name: declMatch[1],
              summary: xmlComment.summary,
              members: [],
            };
          }
        }
      }

      xmlCommentLines = [];
      inXmlComment = false;
      continue;
    }

    // 没有 XML 注释的声明也记录（只记录主要的类型声明）
    if (!inXmlComment && (matchDecl(line, 'class') || matchDecl(line, 'struct') || matchDecl(line, 'interface') || matchDecl(line, 'enum'))) {
      if (line.startsWith('public ') || line.startsWith('internal ')) {
        const declMatch = line.match(/(class|struct|interface|enum)\s+(\w+)/);
        if (declMatch) {
          return {
            filePath,
            namespace,
            module,
            kind: declMatch[1],
            name: declMatch[2],
            summary: xmlCommentLines.length > 0 ? extractXmlComments(xmlCommentLines.join('\n')).summary : '',
            members: [],
          };
        }
      }
    }
  }

  return null;
}

function matchDecl(line, keyword) {
  return new RegExp(`\\b${keyword}\\b`).test(line) &&
    !line.includes('(') &&
    line.includes(keyword);
}

// ── 主流程 ────────────────────────────────────────────────────────────────

function main() {
  // 收集所有 .cs 文件并解析
  const allTypes = [];

  for (const dir of SOURCE_DIRS) {
    if (!fs.existsSync(dir)) continue;
    walkDir(dir, allTypes);
  }

  // 按模块和命名空间分组
  const grouped = {};
  for (const type of allTypes) {
    if (!type) continue;
    const key = type.namespace || 'Unknown';
    if (!grouped[key]) grouped[key] = [];
    grouped[key].push(type);
  }

  // 清空输出目录
  if (fs.existsSync(OUTPUT_DIR)) {
    fs.rmSync(OUTPUT_DIR, { recursive: true });
  }
  fs.mkdirSync(OUTPUT_DIR, { recursive: true });

  // 生成文件
  const sidebarItems = [];

  for (const [ns, types] of Object.entries(grouped).sort()) {
    const nsSlug = ns.replace(/\./g, '-').toLowerCase();
    const lines = [];
    lines.push(`---`);
    lines.push(`sidebar_position: auto`);
    lines.push(`title: ${ns}`);
    lines.push(`---`);
    lines.push('');
    lines.push(`# ${ns}`);
    lines.push('');

    types.sort((a, b) => {
      const order = { class: 1, struct: 2, interface: 3, enum: 4, delegate: 5 };
      return (order[a.kind] || 6) - (order[b.kind] || 6) || a.name.localeCompare(b.name);
    });

    for (const type of types) {
      const kindLabel = {
        class: '类', struct: '结构体', interface: '接口', enum: '枚举', delegate: '委托'
      }[type.kind] || type.kind;

      lines.push(`## ${type.name}`);
      lines.push('');
      lines.push(`**${kindLabel}** | \`${ns}.${type.name}\``);
      lines.push('');

      if (type.summary) {
        lines.push(type.summary);
        lines.push('');
      }

      if (type.members.length > 0) {
        // 方法
        const methods = type.members.filter(m => m.type === 'method');
        if (methods.length > 0) {
          lines.push('### 方法');
          lines.push('');
          for (const m of methods) {
            const sigParts = [];
            if (m.typeParams) sigParts.push(m.typeParams);
            sigParts.push('(');
            sigParts.push(m.params.map(p => `${p.type} ${p.name}`).join(', '));
            sigParts.push(')');
            const sig = sigParts.join('');
            lines.push(`#### \`${m.returnType} ${m.name}${sig}\``);
            lines.push('');
            // 找到对应的 XML 注释
            const xmlDesc = extractXmlCommentsForMember(type.filePath, m.name);
            if (xmlDesc.summary) {
              lines.push(xmlDesc.summary);
              lines.push('');
            }
            if (m.params.some(p => p.desc)) {
              lines.push('| 参数 | 类型 | 说明 |');
              lines.push('|------|------|------|');
              for (const p of m.params) {
                lines.push(`| \`${p.name}\` | \`${p.type}\` | ${p.desc || '-'} |`);
              }
              lines.push('');
            }
            if (m.returns) {
              lines.push(`**返回**: ${m.returns}`);
              lines.push('');
            }
          }
        }

        // 属性
        const properties = type.members.filter(m => m.type === 'property');
        if (properties.length > 0) {
          lines.push('### 属性');
          lines.push('');
          for (const p of properties) {
            lines.push(`- **\`${p.name}\`**${p.summary ? ` — ${p.summary}` : ''}`);
          }
          lines.push('');
        }
      }

      lines.push('---');
      lines.push('');
    }

    // 每个命名空间生成一个 .mdx 文件
    const fileName = `${nsSlug}.mdx`;
    const sidebarOrder = sidebarItems.reduce((sum, s) => sum + s.items.length, 0) + 1;
    const finalLines = [
      `---`,
      `sidebar_position: ${sidebarOrder}`,
      `title: ${ns}`,
      `---`,
      ...lines.slice(4),
    ];
    fs.writeFileSync(path.join(OUTPUT_DIR, fileName), finalLines.join('\n'), 'utf-8');

    // 按模块分类侧边栏
    const fileTypes = types.filter(t => t);
    if (fileTypes.length > 0) {
      const module = fileTypes[0].module || 'Core';
      if (!sidebarItems.find(item => item.module === module)) {
        sidebarItems.push({
          module,
          label: moduleLabel(module),
          items: [],
        });
      }
      const cat = sidebarItems.find(item => item.module === module);
      cat.items.push({
        label: ns,
        id: `generated/${nsSlug}`,
      });
    }
  }

  // 生成侧边栏配置
  const sidebar = sidebarItems.map(cat => ({
    type: 'category',
    label: cat.label,
    items: cat.items.map(i => i.id),
  }));

  fs.writeFileSync(SIDEBAR_FILE, JSON.stringify(sidebar, null, 2), 'utf-8');

  console.log(`Generated API docs for ${allTypes.length} types across ${Object.keys(grouped).length} namespaces.`);
  console.log(`Output: ${OUTPUT_DIR}`);
}

function moduleLabel(module) {
  const labels = {
    'AssetManager': '资源管理',
    'MVC': 'MVC 模式',
    'ObjectPool': '对象池',
    'ReferencePool': '引用池',
    'HFSM': '层次状态机',
    'Procedure': '流程管理',
    'Core': '框架核心',
    'Serialize': '序列化',
    'Sheet': '配置表',
    'Utils': '工具类',
    'Editor-Core': '编辑器核心',
    'Editor-ObjectPool': '编辑器-对象池',
    'Editor-Sheet': '编辑器-配置表',
    'Editor-Addressables': '编辑器-Addressables',
    'Editor-Utils': '编辑器-工具',
  };
  return labels[module] || module;
}

function walkDir(dir, results) {
  const entries = fs.readdirSync(dir, { withFileTypes: true });
  for (const entry of entries) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory() && !entry.name.startsWith('.') && entry.name !== 'obj' && entry.name !== 'bin') {
      walkDir(fullPath, results);
    } else if (entry.isFile() && entry.name.endsWith('.cs')) {
      const parsed = parseCsFile(fullPath);
      if (parsed) {
        results.push(parsed);
      }
    }
  }
}

// 辅助：从文件中提取某个成员的 XML 注释
function extractXmlCommentsForMember(filePath, memberName) {
  try {
    const content = fs.readFileSync(filePath, 'utf-8');
    const lines = content.split('\n');
    let xmlLines = [];
    let collecting = false;

    for (let i = 0; i < lines.length; i++) {
      const line = lines[i].trim();
      if (line.startsWith('///')) {
        collecting = true;
        xmlLines.push(line);
      } else if (collecting && line.includes(memberName)) {
        const result = extractXmlComments(xmlLines.join('\n'));
        xmlLines = [];
        collecting = false;
        return result;
      } else {
        xmlLines = [];
        collecting = false;
      }
    }
  } catch (e) {
    // ignore
  }
  return { summary: '' };
}

main();
