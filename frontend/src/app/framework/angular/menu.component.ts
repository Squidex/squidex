/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { NgTemplateOutlet } from '@angular/common';
import { AfterViewInit, booleanAttribute, ChangeDetectionStrategy, ChangeDetectorRef, Component, ContentChildren, ElementRef, Input, QueryList, ViewChild } from '@angular/core';
import { ModalModel, ResizeListener, ResizeService, Subscriptions } from '@app/framework/internal';
import { DropdownMenuComponent } from './dropdown-menu.component';
import { MenuItemComponent } from './menu-item.component';
import { ModalPlacementDirective } from './modals/modal-placement.directive';
import { ModalDirective } from './modals/modal.directive';
import { TranslatePipe } from './pipes/translate.pipe';

@Component({
    selector: 'sqx-menu',
    styleUrls: ['./menu.component.scss'],
    templateUrl: './menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DropdownMenuComponent,
        ModalDirective,
        ModalPlacementDirective,
        NgTemplateOutlet,
        TranslatePipe,
    ],
})
export class MenuComponent implements AfterViewInit, ResizeListener {
    private readonly subscriptions = new Subscriptions();
    private measuredContainer?: number;
    private measuredMenu?: number;

    @Input()
    public alignment: 'left' | 'right' = 'left';

    @Input({ transform: booleanAttribute })
    public small = false;

    @Input({ transform: booleanAttribute })
    public showCustom = true;

    @ViewChild('container', { static: true })
    public container!: ElementRef<HTMLDivElement>;

    @ViewChild('menu', { static: true })
    public menu!: ElementRef<HTMLDivElement>;

    @ContentChildren(MenuItemComponent)
    public menuItems!: QueryList<MenuItemComponent>;

    public overflowMenuItems: MenuItemComponent[] | null = null;
    public overflowDropdown = new ModalModel();

    public get isRightAligned() {
        return this.alignment === 'right';
    }

    constructor(
        private readonly resizeService: ResizeService,
        private readonly changeDetector: ChangeDetectorRef,
    ) {
    }

    public ngAfterViewInit() {
        this.subscriptions.add(this.resizeService.listen(this.container.nativeElement, this));
        this.subscriptions.add(this.resizeService.listen(this.menu.nativeElement, this));
    }

    public onResize(rect: DOMRect, element: Element): void {
        if (element === this.container.nativeElement) {
            this.measuredContainer = rect.width;
        } else {
            this.measuredMenu = Math.max(rect.width, element.scrollWidth);
        }

        if (!this.measuredContainer || !this.measuredMenu) {
            return;
        }

        const isOverlapping = this.measuredMenu > this.measuredContainer;
        this.overflowMenuItems = isOverlapping ? this.menuItems.toArray().filter(x => x.showInDropdown) : null;
        this.changeDetector.detectChanges();
    }
}