/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ActivatedRoute, ActivatedRouteSnapshot, Data, Params, RouterStateSnapshot } from '@angular/router';

export function allData(value: ActivatedRouteSnapshot | ActivatedRoute): Data {
    let snapshot: ActivatedRouteSnapshot | null = value['snapshot'] || value;

    const result: { [key: string]: any } = {};

    while (snapshot) {
        for (const [key, value] of Object.entries(snapshot.data)) {
            if (!result[key]) {
                result[key] = value;
            }
        }

        snapshot = snapshot.parent;
    }

    return result;
}
export function allParams(value: ActivatedRouteSnapshot | ActivatedRoute): Params {
    let snapshot: ActivatedRouteSnapshot | null = value['snapshot'] || value;

    const result: { [key: string]: any } = {};

    while (snapshot) {
        for (const [key, value] of Object.entries(snapshot.params)) {
            if (!result[key]) {
                result[key] = value;
            }
        }

        snapshot = snapshot.parent;
    }

    return result;
}

export function childComponent(value: RouterStateSnapshot) {
    let current = value.root;

    while (current) {
        if (current.firstChild) {
            current = current.firstChild;
        } else {
            break;
        }
    }

    return current.component;
}
