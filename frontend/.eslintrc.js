/* eslint-disable */

module.exports = {
    "env": {
        "browser": true,
        "node": true
    },
    "extends": [
        "airbnb-typescript/base"
    ],
    "parser": "@typescript-eslint/parser",
    "parserOptions": {
        "project": "tsconfig.json"
    },
    "plugins": [
        "deprecation",
        "eslint-plugin-import",
        "@typescript-eslint",
    ],
    "rules": {
        "deprecation/deprecation": "warn",
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
                "selector": "variable",
                "format": [
                    "camelCase",
                    "PascalCase",
                    "UPPER_CASE",
                ],
                "leadingUnderscore": "allow",
                "trailingUnderscore": "allow",
            },
            {
                "selector": "typeLike",
                "format": [
                    "PascalCase"
                ],
            }
        ],
        "@typescript-eslint/no-this-alias": "error",
        "@typescript-eslint/no-unnecessary-boolean-literal-compare": "error",
        "@typescript-eslint/no-unused-expressions": "off",
        "@typescript-eslint/no-use-before-define": "off",
        "@typescript-eslint/no-shadow": "off",
        "@typescript-eslint/no-unused-vars": [
            "error",
            {
                "argsIgnorePattern": "^_",
                "varsIgnorePattern": "^_"
            }
        ],
        "@typescript-eslint/return-await": "off",
        "@typescript-eslint/quotes": [
            "error",
            "single"
        ],
        "@typescript-eslint/semi": [
            "error",
            "always"
        ],
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
            "pathGroupsExcludedImportTypes": ["builtin"],
            "pathGroups": [{
                "pattern": "@app/**",
                "group": "external",
                "position": "after"
            }],
            "alphabetize": {
                "order": "asc"
            }
        }],
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
                "ObjectExpression": {
                    "consistent": true
                },
                "ObjectPattern": {
                    "consistent": true
                },
                "ImportDeclaration": "never",
                "ExportDeclaration": "never"
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
        ],
    }
};
