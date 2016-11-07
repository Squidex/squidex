/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { BehaviorSubject, Observable } from 'rxjs';

export class ModalView {
    private readonly isOpen$: BehaviorSubject<boolean>;

    public get isOpen(): Observable<boolean> {
        return this.isOpen$.distinctUntilChanged();
    }

    constructor(isOpen = false,
        public readonly closeAlways = false
    ) {
        this.isOpen$ = new BehaviorSubject(isOpen);
    }

    public show() {
        this.isOpen$.next(true);
    }

    public hide() {
        this.isOpen$.next(false);
    }

    public toggle() {
        let value = false;

        this.isOpen.subscribe(v => {
            value = v;
        }).unsubscribe();

        this.isOpen$.next(!value);
    }
}