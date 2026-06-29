import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const config: Config = {
  title: 'HNUnityFramework',
  tagline: '轻量级、模块化的 Unity 游戏开发框架',
  favicon: 'img/favicon.ico',

  future: {
    v4: true,
  },

  url: 'https://haonan9279.github.io',
  baseUrl: '/HNUnityFramework/',

  organizationName: 'HaoNaN9279',
  projectName: 'HNUnityFramework',

  onBrokenLinks: 'warn',
  trailingSlash: true,

  i18n: {
    defaultLocale: 'zh-Hans',
    locales: ['zh-Hans'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          path: 'docs/guide',
          routeBasePath: 'guide',
          sidebarPath: './sidebars-guide.ts',
          sidebarCollapsible: true,
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],
  plugins: [
    // 开发文档插件
    [
      '@docusaurus/plugin-content-docs',
      {
        id: 'dev',
        path: 'docs/dev',
        routeBasePath: 'dev',
        sidebarPath: './sidebars-dev.ts',
        sidebarCollapsible: true,
      },
    ],
    // API 文档
    [
      '@docusaurus/plugin-content-docs',
      {
        id: 'api',
        path: 'docs/api',
        routeBasePath: 'api',
        sidebarPath: './sidebars-api.ts',
        sidebarCollapsible: true,
      },
    ],
  ],

  themeConfig: {
    colorMode: {
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'HNUnityFramework',
      logo: {
        alt: 'HNUnityFramework',
        src: 'img/logo.svg',
      },
      items: [
        {
          to: '/guide',
          position: 'left',
          label: '使用指南',
        },
        {
          to: '/dev',
          position: 'left',
          label: '开发文档',
        },
        {
          to: '/api',
          position: 'left',
          label: 'API 文档',
        },
        {
          href: 'https://github.com/HaoNaN9279/HNUnityFramework',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: '文档',
          items: [
            {
              label: '使用指南',
          to: '/guide',
            },
            {
              label: '开发文档',
          to: '/dev',
            },
            {
              label: 'API 文档',
          to: '/api',
            },
          ],
        },
        {
          title: '链接',
          items: [
            {
              label: 'GitHub',
              href: 'https://github.com/HaoNaN9279/HNUnityFramework',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} HNUnityFramework. Built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
