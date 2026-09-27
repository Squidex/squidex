/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ModalService, OpenModal } from './modal.service';

describe('ModalService', () => {
    let modalService: ModalService;

    beforeEach(() => {
        modalService = new ModalService();
    });

    it('should hide all open modals', () => {
        const modal1 = new TestModal();
        const modal2 = new TestModal();

        modalService.add(modal1);
        modalService.add(modal2);
        modalService.hideAll();

        expect(modal1.isHidden).toBeTruthy();
        expect(modal2.isHidden).toBeTruthy();
    });

    it('should not hide modal that contains the element', () => {
        const element = document.createElement('div');

        const modal1 = new TestModal(element);
        const modal2 = new TestModal();

        modalService.add(modal1);
        modalService.add(modal2);
        modalService.hideAll(element);

        expect(modal1.isHidden).toBeFalsy();
        expect(modal2.isHidden).toBeTruthy();
    });

    it('should not hide removed modal', () => {
        const modal = new TestModal();

        modalService.add(modal);
        modalService.remove(modal);
        modalService.hideAll();

        expect(modal.isHidden).toBeFalsy();
    });
});

class TestModal implements OpenModal {
    public isHidden = false;

    constructor(
        private readonly element?: Element,
    ) {
    }

    public contains(element: Element) {
        return element === this.element;
    }

    public hide() {
        this.isHidden = true;
    }
}
