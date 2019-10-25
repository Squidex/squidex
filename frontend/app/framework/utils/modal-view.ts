/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { BehaviorSubject, Observable } from 'rxjs';

export interface Openable {
    isOpen: Observable<boolean>;
}

export class DialogModel implements Openable {
    private readonly isOpen$: BehaviorSubject<boolean>;

    public get isOpen(): Observable<boolean> {
        return this.isOpen$;
    }

    constructor(isOpen = false) {
        this.isOpen$ = new BehaviorSubject<boolean>(isOpen);
    }

    public show(): DialogModel {
        this.isOpen$.next(true);

        return this;
    }

    public hide(): DialogModel {
        this.isOpen$.next(false);

        return this;
    }

    public toggle(): DialogModel {
        this.isOpen$.next(!this.isOpen$.value);

        return this;
    }
}

export class ModalModel implements Openable {
    private readonly isOpen$: BehaviorSubject<boolean>;

    public get isOpen(): Observable<boolean> {
        return this.isOpen$;
    }

    constructor(isOpen = false) {
        this.isOpen$ = new BehaviorSubject<boolean>(isOpen);
    }

    public show(): ModalModel {
        if (!this.isOpen$.value) {
            if (openModal && openModal !== this) {
                openModal.hide();
            }

            openModal = this;

            this.isOpen$.next(true);
        }

        return this;
    }

    public hide(): ModalModel {
        if (this.isOpen$.value) {
            if (openModal === this) {
                openModal = null;
            }

            this.isOpen$.next(false);
        }

        return this;
    }

    public toggle(): ModalModel {
        if (this.isOpen$.value) {
            this.hide();
        } else {
            this.show();
        }

        return this;
    }
}

let openModal: ModalModel | null = null;