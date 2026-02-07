const {
  defineConfig,
} = require("eslint/config");

const globals = require("globals");
const tsParser = require("@typescript-eslint/parser");
const deprecation = require("eslint-plugin-deprecation");
const importPlugin = require("eslint-plugin-import");
const typescriptEslint = require("@typescript-eslint/eslint-plugin");

const js = require("@eslint/js");

const {
  FlatCompat,
} = require("@eslint/eslintrc");

const compat = new FlatCompat({
  baseDirectory: __dirname,
  recommendedConfig: js.configs.recommended,
  allConfig: js.configs.all
});

module.exports = defineConfig([{
  languageOptions: {
    globals: {
      ...globals.browser,
      ...globals.node,
    },

    parser: tsParser,

    parserOptions: {
      "project": ["tsconfig.app.json", "tsconfig.spec.json"],
      "createDefaultProgram": true,
    },
  },

  extends: compat.extends(
    "eslint:recommended",
    "plugin:@angular-eslint/recommended",
    "plugin:@angular-eslint/template/process-inline-templates",
  ),

  plugins: {
    deprecation,
    import: importPlugin,
    "@typescript-eslint": typescriptEslint,
  },
},
{
  files: ['src/**/*.ts', '**/*.ts'],
  rules: {
    "@angular-eslint/component-selector": ["error", {
      "prefix": "sqx",
      "style": "kebab-case",
      "type": "element",
    }],

    "@angular-eslint/directive-selector": ["error", {
      "prefix": "sqx",
      "style": "camelCase",
      "type": "attribute",
    }],

    "@angular-eslint/prefer-inject": ["off"],
    "@angular-eslint/use-lifecycle-interface": ["off"],
    "@typescript-eslint/dot-notation": "off",
    "@typescript-eslint/indent": "off",
    "@typescript-eslint/lines-between-class-members": "off",

    "@typescript-eslint/naming-convention": ["error", {
      "format": ["camelCase", "PascalCase", "UPPER_CASE"],
      "leadingUnderscore": "allow",
      "selector": "variable",
      "trailingUnderscore": "allow",
    }, {
        "format": ["PascalCase"],
        "selector": "typeLike",
      }],

    "@typescript-eslint/no-shadow": "off",
    "@typescript-eslint/no-this-alias": "error",
    "@typescript-eslint/no-unnecessary-boolean-literal-compare": "error",
    "@typescript-eslint/no-unused-expressions": "off",
    "@typescript-eslint/no-implied-eval": "error",

    "@typescript-eslint/no-unused-vars": ["error", {
      "argsIgnorePattern": "^_",
      "varsIgnorePattern": "^_",
    }],

    "@typescript-eslint/no-use-before-define": "off",
    "@typescript-eslint/return-await": "off",
    "arrow-body-style": "off",
    "arrow-parens": "off",
    "class-methods-use-this": "off",
    "default-case": "off",
    "function-paren-newline": "off",
    "implicit-arrow-linebreak": "off",
    "import/extensions": "off",
    "import/no-extraneous-dependencies": "off",
    "import/no-useless-path-segments": "off",

    "import/order": ["error", {
      "alphabetize": {
        "order": "asc",
      },

      "pathGroups": [{
        "group": "external",
        "pattern": "@app/**",
        "position": "after",
      }],

      "pathGroupsExcludedImportTypes": ["builtin"],
    }],

    "import/prefer-default-export": "off",
    "linebreak-style": "off",
    "max-classes-per-file": "off",
    "max-len": "off",
    "newline-per-chained-call": "off",
    "no-else-return": "off",
    "no-extra-boolean-cast": "off",
    "no-mixed-operators": "off",
    "no-nested-ternary": "off",
    "no-param-reassign": "off",
    "no-plusplus": "off",
    "no-prototype-builtins": "off",
    "no-restricted-syntax": "off",
    "no-trailing-spaces": "error",
    "no-underscore-dangle": "off",
    "no-undef": "off",
    "no-unused-vars": "off",
    "no-useless-escape": "off",
    "no-useless-return": "warn",

    "object-curly-newline": ["error", {
      "ExportDeclaration": "never",
      "ImportDeclaration": "never",

      "ObjectExpression": {
        "consistent": true,
      },

      "ObjectPattern": {
        "consistent": true,
      },
    }],

    "operator-linebreak": "off",
    "prefer-destructuring": "off",

    "sort-imports": ["error", {
      "ignoreCase": true,
      "ignoreDeclarationSort": true,
    }],
  },
}]);
