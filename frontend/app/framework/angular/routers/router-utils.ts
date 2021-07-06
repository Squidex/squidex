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
        for (const key in snapshot.data) {
            if (snapshot.data.hasOwnProperty(key) && !result[key]) {
                result[key] = snapshot.data[key];
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
        for (const key in snapshot.params) {
            if (snapshot.params.hasOwnProperty(key) && !result[key]) {
                result[key] = snapshot.params[key];
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
