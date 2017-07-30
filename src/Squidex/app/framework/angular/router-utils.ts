/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ActivatedRoute, ActivatedRouteSnapshot, Data, Params } from '@angular/router';

export function allData(value: ActivatedRouteSnapshot | ActivatedRoute): Data {
    let snapshot: ActivatedRouteSnapshot = value['snapshot'] || value;

    const result: { [key: string]: any } = { };

    while (snapshot) {
        for (let key in snapshot.data) {
            if (snapshot.data.hasOwnProperty(key) && !result[key]) {
                result[key] = snapshot.data[key];
            }
        }

        snapshot = snapshot.parent;
    }

    return result;
}
export function allParams(value: ActivatedRouteSnapshot | ActivatedRoute): Params {
    let snapshot: ActivatedRouteSnapshot = value['snapshot'] || value;

    const result: { [key: string]: any } = { };

    while (snapshot) {
        for (let key in snapshot.params) {
            if (snapshot.params.hasOwnProperty(key) && !result[key]) {
                result[key] = snapshot.params[key];
            }
        }

        snapshot = snapshot.parent;
    }

    return result;
}