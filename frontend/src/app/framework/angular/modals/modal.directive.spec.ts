/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { DialogModel, ModalModel, ModalService } from '@app/framework/internal';
import { ModalDirective } from './modal.directive';
import { RootViewComponent } from './root-view.component';

@Component({
    template: `
        <ng-container *sqxModal="dialog1; onRoot: false">
            <div id="anchor"></div>
        </ng-container>
        <ng-container *sqxModal="dialog2; onRoot: false">
            <div></div>
        </ng-container>
        <ng-container *sqxModal="dropdown; onRoot: false">
            <div></div>
        </ng-container>
    `,
    imports: [
        ModalDirective,
    ],
})
class TestHostComponent {
    public dialog1 = new DialogModel();
    public dialog2 = new DialogModel();
    public dropdown = new ModalModel();
}

describe('ModalDirective', () => {
    let host: TestHostComponent;
    let modalService: ModalService;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [TestHostComponent],
            providers: [
                { provide: RootViewComponent, useValue: {} },
            ],
        });

        const fixture = TestBed.createComponent(TestHostComponent);

        fixture.detectChanges();

        host = fixture.componentInstance;
        modalService = TestBed.inject(ModalService);

        host.dialog1.show();
        host.dialog2.show();
        host.dropdown.show();
    });

    it('should hide all open modals', () => {
        modalService.hideAll();

        expect(host.dialog1.isOpen).toBeFalsy();
        expect(host.dialog2.isOpen).toBeFalsy();
        expect(host.dropdown.isOpen).toBeFalsy();
    });

    it('should not hide modal that contains the element', () => {
        const anchor = document.getElementById('anchor');

        modalService.hideAll(anchor);

        expect(host.dialog1.isOpen).toBeTruthy();
        expect(host.dialog2.isOpen).toBeFalsy();
        expect(host.dropdown.isOpen).toBeFalsy();
    });
});
