/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, ActivatedRouteSnapshot, Data, Params, RouterStateSnapshot } from '@angular/router';
import { distinctUntilChanged, map } from 'rxjs';

export function routeParam(name: string, route = inject(ActivatedRoute)): Signal<string | null> {
    const values = route.paramMap.pipe(map(x => x.get(name)), distinctUntilChanged());

    return toSignal(values, { initialValue: null });
}

export function queryParam<T extends string = string>(name: string, defaultValue: NoInfer<T>, route = inject(ActivatedRoute)): Signal<T> {
    const values = route.queryParamMap.pipe(map(x => (x.get(name) || defaultValue) as T), distinctUntilChanged());

    return toSignal(values, { initialValue: defaultValue });
}

export function allData(value: ActivatedRouteSnapshot | ActivatedRoute): Data {
    let snapshot: ActivatedRouteSnapshot | null = (value as any)['snapshot'] || value;

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
    let snapshot: ActivatedRouteSnapshot | null = (value as any)['snapshot'] || value;

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
