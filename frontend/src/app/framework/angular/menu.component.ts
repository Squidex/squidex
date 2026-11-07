/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgTemplateOutlet } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, ElementRef, Injectable, input, Optional, signal, SkipSelf, viewChild } from '@angular/core';
import { ModalModel, ResizeListener, ResizeService } from '@app/framework/internal';
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
export class MenuComponent implements ResizeListener {
    private readonly menuItemsRegistry: MenuItemRegistry;
    private readonly measuredContainer = signal(-1);
    private readonly measuredMenu = signal(-1);
    private readonly container = viewChild<ElementRef<HTMLDivElement>>('container');
    private readonly menu = viewChild<ElementRef<HTMLDivElement>>('menu');

    /** Aligns the menu items and the overflow button to the left or right. */
    public readonly alignment = input<'left' | 'right'>('left');

    /** Renders small buttons with less space between them. */
    public readonly small = input(false, { transform: booleanAttribute });

    public readonly overflowDropdown = new ModalModel();

    public readonly isRightAligned = computed(() => {
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

    constructor(
        resizeService: ResizeService,
        @Optional() @SkipSelf() parentMenuItemRegistry?: MenuItemRegistry,
    ) {
        this.menuItemsRegistry = parentMenuItemRegistry ?? new MenuItemRegistry();

        effect(onCleanup => {
            const container = this.container();
            if (!container) {
                return;
            }

            const unlistenContainer = resizeService.listen(container.nativeElement, this);
            onCleanup(unlistenContainer);
        });

        effect(onCleanup => {
            const menu = this.menu();
            if (!menu) {
                return;
            }

            const unlistenMenu = resizeService.listen(menu.nativeElement, this);
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