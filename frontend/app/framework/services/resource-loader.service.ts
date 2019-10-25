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
            result = new Promise((resolve, reject) => {
                const style = this.renderer.createElement('link');

                this.renderer.listen(style, 'load', () => resolve());
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
            result = new Promise((resolve, reject) => {
                const script = this.renderer.createElement('script');

                this.renderer.listen(script, 'load', () => resolve());
                this.renderer.setProperty(script, 'src', url);
                this.renderer.setProperty(script, 'async', true);
                this.renderer.appendChild(document.body, script);
            });

            this.cache[key] = result;
        }

        return result;
    }
}