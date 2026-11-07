/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Signal, signal } from '@angular/core';
import { Placement } from '@floating-ui/dom';
import { BehaviorSubject, Observable } from 'rxjs';

export type FloatingPlacement = Placement | [Placement, 'inside' | 'outside'];

export interface Openable {
    isOpenChanges: Observable<boolean>;

    isOpen: Signal<boolean>;
}

abstract class OpenableBase implements Openable {
    private readonly isOpen$: BehaviorSubject<boolean>;
    private readonly isOpenState;

    public readonly isOpen: Signal<boolean>;

    public get isOpenChanges(): Observable<boolean> {
        return this.isOpen$;
    }

    constructor(isOpen: boolean) {
        this.isOpen$ = new BehaviorSubject<boolean>(isOpen);
        this.isOpenState = signal(isOpen);
        this.isOpen = this.isOpenState.asReadonly();
    }

    protected setOpen(isOpen: boolean) {
        this.isOpenState.set(isOpen);
        this.isOpen$.next(isOpen);
    }
}

export class DialogModel extends OpenableBase {
    constructor(isOpen = false) {
        super(isOpen);
    }

    public show(): DialogModel {
        this.setOpen(true);

        return this;
    }

    public hide(): DialogModel {
        this.setOpen(false);

        return this;
    }

    public toggle(): DialogModel {
        this.setOpen(!this.isOpen());

        return this;
    }
}

export class ModalModel extends OpenableBase {
    private static openModal?: ModalModel | null = null;

    constructor(isOpen = false) {
        super(isOpen);
    }

    public show(): ModalModel {
        if (!this.isOpen()) {
            if (ModalModel.openModal && ModalModel.openModal !== this) {
                ModalModel.openModal.hide();
            }

            ModalModel.openModal = this;

            this.setOpen(true);
        }

        return this;
    }

    public hide(): ModalModel {
        if (this.isOpen()) {
            if (ModalModel.openModal === this) {
                ModalModel.openModal = null;
            }

            this.setOpen(false);
        }

        return this;
    }

    public toggle(): ModalModel {
        if (this.isOpen()) {
            this.hide();
        } else {
            this.show();
        }

        return this;
    }
}
