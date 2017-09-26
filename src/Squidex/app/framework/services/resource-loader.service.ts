/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';

@Injectable()
export class ResourceLoaderService {
    private cache: { [path: string]: Promise<any> } = {};

    public loadStyle(url: string): Promise<any> {
        const key = url.toUpperCase();

        let result = this.cache[key];

        if (!result) {
            result = new Promise((resolve, reject) => {
                const style = document.createElement('link');
                style.rel  = 'stylesheet';
                style.href = url;
                style.type = 'text/css';

                style.onload = () => {
                    resolve();
                };

                const head = document.getElementsByTagName('head')[0];

                head.appendChild(style);
            });

            this.cache[key] = result;
        }

        return result;
    }

    public loadScript(url: string): Promise<any> {
        const key = url.toUpperCase();

        let result = this.cache[key];

        if (!result) {
            result = new Promise((resolve, reject) => {
                const script = document.createElement('script');
                script.src = url;
                script.async = true;

                script.onload = () => {
                    resolve();
                };

                const node = document.getElementsByTagName('script')[0];

                if (node.parentNode) {
                    node.parentNode.insertBefore(script, node);
                }
            });

            this.cache[key] = result;
        }

        return result;
    }
}