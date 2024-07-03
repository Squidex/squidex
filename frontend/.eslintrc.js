/* eslint-disable */

module.exports = {
    "env": {
        "browser": true,
        "node": true
    },
    "extends": [
        "airbnb-typescript/base",
        "plugin:@angular-eslint/recommended",
        "plugin:@angular-eslint/template/process-inline-templates"
    ],
    "parser": "@typescript-eslint/parser",
    "parserOptions": {
        "project": "tsconfig.json"
    },
    "plugins": [
        "deprecation",
        "eslint-plugin-import",
        "@typescript-eslint"
    ],
    "rules": {
        "@angular-eslint/component-selector": [
            "error",
            {
                "prefix": "sqx",
                "style": "kebab-case",
                "type": "element"
            }
        ],
        "@angular-eslint/directive-selector": [
            "error",
            {
                "prefix": "sqx",
                "style": "camelCase",
                "type": "attribute"
            }
        ],
        "@angular-eslint/use-lifecycle-interface": [
            "off"
        ],
        "@typescript-eslint/dot-notation": "off",
        "@typescript-eslint/indent": "off",
        "@typescript-eslint/lines-between-class-members": "off",
        "@typescript-eslint/member-delimiter-style": [
            "error",
            {
                "multiline": {
                    "delimiter": "semi",
                    "requireLast": true
                },
                "singleline": {
                    "delimiter": "semi",
                    "requireLast": false
                }
            }
        ],
        "@typescript-eslint/naming-convention": [
            "error",
            {
                "format": [
                    "camelCase",
                    "PascalCase",
                    "UPPER_CASE"
                ],
                "leadingUnderscore": "allow",
                "selector": "variable",
                "trailingUnderscore": "allow"
            },
            {
                "format": [
                    "PascalCase"
                ],
                "selector": "typeLike"
            }
        ],
        "@typescript-eslint/no-shadow": "off",
        "@typescript-eslint/no-this-alias": "error",
        "@typescript-eslint/no-unnecessary-boolean-literal-compare": "error",
        "@typescript-eslint/no-unused-expressions": "off",
        "@typescript-eslint/no-unused-vars": [
            "error",
            {
                "argsIgnorePattern": "^_",
                "varsIgnorePattern": "^_"
            }
        ],
        "@typescript-eslint/no-use-before-define": "off",
        "@typescript-eslint/quotes": [
            "error",
            "single"
        ],
        "@typescript-eslint/return-await": "off",
        "@typescript-eslint/semi": [
            "error",
            "always"
        ],
        "arrow-body-style": "off",
        "arrow-parens": "off",
        "class-methods-use-this": "off",
        "default-case": "off",
        "deprecation/deprecation": "warn",
        "function-paren-newline": "off",
        "implicit-arrow-linebreak": "off",
        "import/extensions": "off",
        "import/no-extraneous-dependencies": "off",
        "import/no-useless-path-segments": "off",
        "import/order": [
            "error",
            {
                "alphabetize": {
                    "order": "asc"
                },
                "pathGroups": [
                    {
                        "group": "external",
                        "pattern": "@app/**",
                        "position": "after"
                    }
                ],
                "pathGroupsExcludedImportTypes": [
                    "builtin"
                ]
            }
        ],
        "import/prefer-default-export": "off",
        "linebreak-style": "off",
        "max-classes-per-file": "off",
        "max-len": "off",
        "newline-per-chained-call": "off",
        "no-else-return": "off",
        "no-mixed-operators": "off",
        "no-nested-ternary": "off",
        "no-param-reassign": "off",
        "no-plusplus": "off",
        "no-prototype-builtins": "off",
        "no-restricted-syntax": "off",
        "no-trailing-spaces": "error",
        "no-underscore-dangle": "off",
        "object-curly-newline": [
            "error",
            {
                "ExportDeclaration": "never",
                "ImportDeclaration": "never",
                "ObjectExpression": {
                    "consistent": true
                },
                "ObjectPattern": {
                    "consistent": true
                }
            }
        ],
        "operator-linebreak": "off",
        "prefer-destructuring": "off",
        "sort-imports": [
            "error",
            {
                "ignoreCase": true,
                "ignoreDeclarationSort": true
            }
        ]
    }
};
