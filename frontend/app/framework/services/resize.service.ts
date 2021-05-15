/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable, OnDestroy } from '@angular/core';
import ResizeObserver from 'resize-observer-polyfill';

export interface ResizeListener {
    onResize(rect: DOMRect, element: Element): void;
}

@Injectable()
export class ResizeService implements OnDestroy {
    private readonly listeners = new WeakMap<Element, ResizeListener>();
    private observer: ResizeObserver;

    public ngOnDestroy() {
        if (this.observer) {
            this.observer.disconnect();
        }
    }

    public listen(target: Element, listener: ResizeListener) {
        if (!this.observer) {
            this.observer = new ResizeObserver((entries: ResizeObserverEntry[]) => {
                for (const entry of entries) {
                    if (this.listeners.has(entry.target)) {
                        const component = this.listeners.get(entry.target);

                        if (component) {
                            component.onResize(entry.contentRect as any, entry.target);
                        }
                    }
                }
            });
        }

        this.listeners.set(target, listener);

        this.observer.observe(target);

        return () => {
            this.unlisten(target);
        };
    }

    public unlisten(target: Element) {
        this.observer.unobserve(target);
    }
}
