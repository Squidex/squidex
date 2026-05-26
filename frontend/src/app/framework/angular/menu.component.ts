/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { NgTemplateOutlet } from '@angular/common';
import { AfterViewInit, booleanAttribute, ChangeDetectionStrategy, Component, computed, ElementRef, Injectable, Input, Optional, signal, SkipSelf, ViewChild } from '@angular/core';
import { ModalModel, ResizeListener, ResizeService, Subscriptions } from '@app/framework/internal';
import { DropdownMenuComponent } from './dropdown-menu.component';
import { MenuItemComponent } from './menu-item.component';
import { ModalPlacementDirective } from './modals/modal-placement.directive';
import { ModalDirective } from './modals/modal.directive';
import { TranslatePipe } from './pipes/translate.pipe';

@Injectable()
export class MenuItemRegistry {
    public readonly menuItems = signal(new Set<MenuItemComponent>());

    public registerItem(item: MenuItemComponent) {
        this.menuItems.update(x => {
            const update = new Set<MenuItemComponent>(x);
            update.add(item);
            return update;
        });
    }

    public unregisterItem(item: MenuItemComponent) {
        this.menuItems.update(x => {
            const update = new Set<MenuItemComponent>(x);
            update.delete(item);
            return update;
        });
    }
}

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
    providers: [{
        provide: MenuItemRegistry,
        useFactory: (menu: MenuComponent) => menu.menuItemsRegistry,
        deps: [MenuComponent],
    }]
})
export class MenuComponent implements AfterViewInit, ResizeListener {
    private readonly subscriptions = new Subscriptions();
    private readonly menuItemsRegistry: MenuItemRegistry;
    private readonly measuredContainer = signal(-1);
    private readonly measuredMenu = signal(-1);

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

    public overflowDropdown = new ModalModel();

    public get isRightAligned() {
        return this.alignment === 'right';
    }

    constructor(
        private readonly resizeService: ResizeService,
        @Optional() @SkipSelf() parentMenuItemRegistry?: MenuItemRegistry,
    ) {
        this.menuItemsRegistry = parentMenuItemRegistry ?? new MenuItemRegistry();
    }

    public ngAfterViewInit() {
        this.subscriptions.add(this.resizeService.listen(this.container.nativeElement, this));
        this.subscriptions.add(this.resizeService.listen(this.menu.nativeElement, this));
    }

    public onResize(rect: DOMRect, element: Element): void {
        if (element === this.container.nativeElement) {
            this.measuredContainer.set(rect.width);
        } else {
            this.measuredMenu.set(Math.max(rect.width, element.scrollWidth));
        }
    }

    protected overflowMenuItems = computed(() => {
        const items = this.menuItemsRegistry.menuItems();
        const measuredContainer = this.measuredContainer();
        const measuredMenu = this.measuredMenu();

        if (measuredContainer < 0 || measuredMenu < 0) {
            return null;
        }

        const isOverlapping = measuredMenu > measuredContainer;
        return isOverlapping ? [...items.values()].filter(x => x.actualMenuLabel).sortedByString(x => x.actualMenuLabel) : null;
    });
}