/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';

export interface OpenModal {
    contains(element: Element): boolean;

    hide(): void;
}

@Injectable({
    providedIn: 'root',
})
export class ModalService {
    private readonly openModals = new Set<OpenModal>();

    public add(modal: OpenModal) {
        this.openModals.add(modal);
    }

    public remove(modal: OpenModal) {
        this.openModals.delete(modal);
    }

    public hideAll(except?: Element | null) {
        // Hiding a modal removes it from the set, therefore iterate over a copy.
        for (const modal of [...this.openModals]) {
            // Keep the modal that contains the element, otherwise the element would be removed as well.
            if (except && modal.contains(except)) {
                continue;
            }

            modal.hide();
        }
    }
}
