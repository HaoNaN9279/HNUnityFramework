import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

let generatedSidebar = [];
try {
  generatedSidebar = require('./docs/api/generated-sidebar.json');
} catch {
  // Generated API docs not yet created (run `npm run docs:api`)
}

const apiSidebarItems: any[] = [
  'index',
  'reference',
];

if (generatedSidebar.length > 0) {
  apiSidebarItems.push({
    type: 'category',
    label: 'API 参考',
    collapsible: true,
    collapsed: false,
    items: generatedSidebar,
  });
}

const sidebars: SidebarsConfig = {
  apiSidebar: apiSidebarItems,
};

export default sidebars;
