/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ActivatedRoute, ActivatedRouteSnapshot, Data, Params } from '@angular/router';

export function allDataFromRoute(route: ActivatedRoute): Data {
    return allData(route.snapshot);
}

export function allData(route: ActivatedRouteSnapshot): Data {
    let result: { [key: string]: any } = { };

    while (route) {
        for (let key in route.data) {
            if (route.data.hasOwnProperty(key) && !result[key]) {
                result[key] = route.data[key];
            }
        }

        route = route.parent;
    }

    return result;
}

export function allParametersFromRoute(route: ActivatedRoute): Params {
    return allParameters(route.snapshot);
}

export function allParameters(route: ActivatedRouteSnapshot): Params {
    let result: { [key: string]: any } = { };

    while (route) {
        for (let key of route.paramMap.keys) {
            if (!result[key]) {
                result[key] = route.paramMap.get(key);
            }
        }

        route = route.parent;
    }

    return result;
}