/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Types } from './types';

export class Permission {
    private readonly parts: ({ [key: string]: true } | null)[];

    constructor(
        public readonly id: string
    ) {
        this.parts = id.split('.').map(x => {
            if (x === '*') {
                return null;
            } else {
                const result: { [key: string]: true } = {};

                for (let p of x.split('|')) {
                    result[p] = true;
                }

                return result;
            }
        });
    }

    public allows(permission?: Permission | string) {
        if (!permission) {
            return false;
        }

        if (Types.isString(permission)) {
            permission = new Permission(permission);
        }

        if (this.parts.length > permission.parts.length) {
            return false;
        }

        for (let i = 0; i < this.parts.length; i++) {
            let lhs = this.parts[i];
            let rhs = permission.parts[i];

            if (lhs !== null && (rhs === null || !Permission.any(lhs, rhs))) {
                return false;
            }
        }

        return true;
    }

    private static any(lhs: { [key: string]: true }, rhs: { [key: string]: true }) {
        for (let key in lhs) {
            if (rhs[key]) {
                return true;
            }
        }

        for (let key in rhs) {
            if (lhs[key]) {
                return true;
            }
        }

        return false;
    }
}

export function permissionsAllow(permissions: Permission[], other: Permission) {
    if (!other) {
        return false;
    }

    for (let permission of permissions) {
        if (permission.allows(other)) {
            return true;
        }
    }

    return false;
}