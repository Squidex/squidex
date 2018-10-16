/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ActivatedRoute, ActivatedRouteSnapshot, Data, Params, Router, RouterEvent, RouterStateSnapshot, RoutesRecognized } from '@angular/router';

import { Types } from './../../utils/types';

export function allData(value: ActivatedRouteSnapshot | ActivatedRoute): Data {
    let snapshot: ActivatedRouteSnapshot | null = value['snapshot'] || value;

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
    let snapshot: ActivatedRouteSnapshot | null = value['snapshot'] || value;

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

export function childComponent(value: RouterStateSnapshot) {
    let current = value.root;

    while (true) {
        if (current.firstChild) {
            current = current.firstChild;
        } else {
            break;
        }
    }

    return current.component;
}

export function navigatedToOtherComponent(router: Router) {
    return (e: RouterEvent) => Types.is(e, RoutesRecognized) && childComponent(e.state) !== childComponent(router.routerState.snapshot);
}