/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { LocalizerService, ResizeService } from '@app/framework/internal';
import { MenuItemComponent } from './menu-item.component';
import { MenuComponent, MenuItemRegistry } from './menu.component';

@Component({
    template: `
        <sqx-menu>
            <sqx-menu-item label="Item1" />
            @if (showSecond()) {
                <sqx-menu-item label="Item2" menuLabel="Menu2" />
            }
        </sqx-menu>
    `,
    imports: [
        MenuComponent,
        MenuItemComponent,
    ],
})
class TestHostComponent {
    public readonly showSecond = signal(true);
}

describe('MenuComponent', () => {
    beforeEach(() => {
        TestBed.configureTestingModule({
            providers: [
                { provide: LocalizerService, useValue: new LocalizerService({}) },
                { provide: ResizeService, useValue: { listen: () => () => {} } },
            ],
        });
    });

    it('should register menu items', () => {
        const fixture = TestBed.createComponent(TestHostComponent);

        fixture.detectChanges();

        expect(getMenuLabels(fixture.debugElement.query(x => x.componentInstance instanceof MenuComponent).injector.get(MenuItemRegistry))).toEqual(['Item1', 'Menu2']);
    });

    it('should unregister menu items if destroyed', () => {
        const fixture = TestBed.createComponent(TestHostComponent);

        fixture.detectChanges();
        fixture.componentInstance.showSecond.set(false);
        fixture.detectChanges();

        expect(getMenuLabels(fixture.debugElement.query(x => x.componentInstance instanceof MenuComponent).injector.get(MenuItemRegistry))).toEqual(['Item1']);
    });

    function getMenuLabels(registry: MenuItemRegistry) {
        return [...registry.menuItems().values()].map(x => x.actualMenuLabel());
    }
});
