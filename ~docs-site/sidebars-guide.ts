import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  guideSidebar: [
    'index',
    {
      type: 'category',
      label: '快速入门',
      items: ['install', 'quick-start'],
    },
  ],
};

export default sidebars;
