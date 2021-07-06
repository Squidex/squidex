/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable, Renderer2, RendererFactory2 } from '@angular/core';

@Injectable()
export class ResourceLoaderService {
    private readonly cache: { [path: string]: Promise<any> } = {};
    private readonly renderer: Renderer2;

    constructor(rendererFactory: RendererFactory2) {
        this.renderer = rendererFactory.createRenderer(null, null);
    }

    public loadStyle(url: string): Promise<any> {
        const key = url.toUpperCase();

        let result = this.cache[key];

        if (!result) {
            result = new Promise(resolve => {
                const style = this.renderer.createElement('link');

                this.renderer.listen(style, 'load', resolve);
                this.renderer.setProperty(style, 'rel', 'stylesheet');
                this.renderer.setProperty(style, 'href', url);
                this.renderer.setProperty(style, 'type', 'text/css');
                this.renderer.appendChild(document.head, style);
            });

            this.cache[key] = result;
        }

        return result;
    }

    public loadScript(url: string): Promise<any> {
        const key = url.toUpperCase();

        let result = this.cache[key];

        if (!result) {
            const script = this.renderer.createElement('script');

            this.renderer.setProperty(script, 'src', url);
            this.renderer.setProperty(script, 'async', false);
            this.renderer.appendChild(document.body, script);

            result = new Promise((resolve) => {
                this.renderer.listen(script, 'load', resolve);
            });

            this.cache[key] = result;
        }

        return result;
    }

    public loadLocalScript(url: string): Promise<any> {
        return process.env.NODE_ENV !== 'production' ?
            this.loadScript(`https://localhost:3000/${url}`) :
            this.loadScript(`build/${url}`);
    }

    public loadLocalStyle(url: string): Promise<any> {
        return process.env.NODE_ENV !== 'production' ?
            this.loadStyle(`https://localhost:3000/${url}`) :
            this.loadStyle(`build/${url}`);
    }
}
