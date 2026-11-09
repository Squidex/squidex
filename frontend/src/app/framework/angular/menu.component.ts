/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgTemplateOutlet } from '@angular/common';
import { booleanAttribute, Component, computed, effect, ElementRef, inject, input, Service, signal, viewChild } from '@angular/core';
import { ModalModel, ResizeListener, ResizeService } from '@app/framework/internal';
import { DropdownMenuComponent } from './dropdown-menu.component';
import { MenuItemComponent } from './menu-item.component';
import { ModalPlacementDirective } from './modals/modal-placement.directive';
import { ModalDirective } from './modals/modal.directive';
import { TranslatePipe } from './pipes/translate.pipe';

@Service({ autoProvided: false })
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
export class MenuComponent implements ResizeListener {
    private readonly menuItemRegistry = inject(MenuItemRegistry, { optional: true, skipSelf: true });
    private readonly menuItemsRegistry = this.menuItemRegistry ?? new MenuItemRegistry();
    private readonly resizeService = inject(ResizeService);

    /** Aligns the menu items and the overflow button to the left or right. */
    public readonly alignment = input<'left' | 'right'>('left');

    /** Renders small buttons with less space between them. */
    public readonly small = input(false, { transform: booleanAttribute });

    protected readonly measuredContainer = signal(-1);

    protected readonly measuredMenu = signal(-1);

    protected readonly container = viewChild<ElementRef<HTMLDivElement>>('container');

    protected readonly menu = viewChild<ElementRef<HTMLDivElement>>('menu');

    protected readonly isRightAligned = computed(() => {
        return this.alignment() === 'right';
    });

    protected readonly overflowMenuItems = computed(() => {
        const items = this.menuItemsRegistry.menuItems();
        const measuredContainer = this.measuredContainer();
        const measuredMenu = this.measuredMenu();

        if (measuredContainer < 0 || measuredMenu < 0) {
            return null;
        }

        const isOverlapping = measuredMenu > measuredContainer;
        return isOverlapping ? [...items.values()].filter(x => x.actualMenuLabel()).sortedByString(x => x.actualMenuLabel()) : null;
    });

    protected readonly overflowDropdown = new ModalModel();

    constructor() {
        effect(onCleanup => {
            const container = this.container();
            if (!container) {
                return;
            }

            const unlistenContainer = this.resizeService.listen(container.nativeElement, this);
            onCleanup(unlistenContainer);
        });

        effect(onCleanup => {
            const menu = this.menu();
            if (!menu) {
                return;
            }

            const unlistenMenu = this.resizeService.listen(menu.nativeElement, this);
            onCleanup(unlistenMenu);
        });
    }

    public onResize(rect: DOMRect, element: Element): void {
        if (element === this.container()?.nativeElement) {
            this.measuredContainer.set(rect.width);
        } else {
            this.measuredMenu.set(Math.max(rect.width, element.scrollWidth));
        }
    }
}