const { defineConfig } = require('eslint/config');
const expoConfig = require('eslint-config-expo/flat');
const prettier = require('eslint-config-prettier');

module.exports = defineConfig([
  expoConfig,
  prettier,
  {
    ignores: ['node_modules/', '.expo/', 'dist/', 'web-build/', '**/__tests__/**'],
  },
  {
    rules: {
      // axios default export is the Metro-safe import style
      'import/no-named-as-default-member': 'off',
    },
  },
]);
