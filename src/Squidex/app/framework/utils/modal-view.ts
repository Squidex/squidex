/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { BehaviorSubject, Observable } from 'rxjs';

export class ModalView {
    private readonly isOpen$: BehaviorSubject<boolean>;
    private static openView: ModalView | null = null;

    public get isOpen(): Observable<boolean> {
        return this.isOpen$;
    }

    constructor(isOpen = false,
        public readonly closeAlways: boolean = false
    ) {
        this.isOpen$ = new BehaviorSubject(isOpen);
    }

    public show() {
        if (ModalView.openView !== this && ModalView.openView) {
            ModalView.openView.hide();
        }

        ModalView.openView = this;

        this.isOpen$.next(true);
    }

    public hide() {
        if (ModalView.openView === this) {
            ModalView.openView = null;
        }

        this.isOpen$.next(false);
    }

    public toggle() {
        let isOpenSnapshot = false;

        this.isOpen.subscribe(v => {
            isOpenSnapshot = v;
        }).unsubscribe();

        if (isOpenSnapshot) {
            this.hide();
        } else {
            this.show();
        }
    }
}