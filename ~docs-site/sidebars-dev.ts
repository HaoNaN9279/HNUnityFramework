import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  devSidebar: [
    'index',
    'architecture',
    {
      type: 'category',
      label: '贡献指南',
      items: ['contribute', 'code-standard'],
    },
  ],
};

export default sidebars;
