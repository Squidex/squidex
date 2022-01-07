/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export module Cookies {
    export function set(name: string, value: string, days: number) {
        let expires = '';

        if (days) {
            const date = new Date();

            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));

            expires = `; expires=${date.toUTCString()}`;
        }

        document.cookie = `${name}=${value || ''}${expires}; path=/`;
    }

    export function replace(name: string, value: string, days: number) {
        remove(name);

        set(name, value, days);
    }

    export function remove(name: string) {
        document.cookie = `${name}=; Max-Age=-99999999;`;
    }
}
