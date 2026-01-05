/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AsyncPipe } from '@angular/common';
import { AfterViewInit, booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, Input, signal, ViewChild } from '@angular/core';
import { combineLatest, map, Observable, of } from 'rxjs';
import { ModalModel, ResizeListener, ResizeService, Subscriptions, Types } from '@app/framework/internal';
import { DropdownMenuComponent } from './dropdown-menu.component';
import { ConfirmClickDirective } from './forms/confirm-click.directive';
import { ModalPlacementDirective } from './modals/modal-placement.directive';
import { ModalDirective } from './modals/modal.directive';
import { TooltipDirective } from './modals/tooltip.directive';
import { TranslatePipe } from './pipes/translate.pipe';

export interface MenuItemBase {
    key: string;

    // The icon of te menu item.
    icon?: string;

    // The label.
    label?: string;

    // The menu label.
    menuLabel?: string;

    // The tooltip.
    tooltip?: string;

    // The confirm title.
    confirmTitle?: string | null;

    // The confirm text.
    confirmText?: string | null;

    // The confirm remember key.
    confirmRememberKey?: string | null;

    // True, to always show it.
    showAlways?: boolean;

    // The tab index.
    tabIndex?: number;

    // The action when the tooltip is clicked.
    onClick: () => void;
}

type BooleanInput = boolean | Observable<boolean> | undefined | null;

export interface MenuItem extends MenuItemBase {
    // Indicates if the menu item is disabled.
    isDisabled?: BooleanInput;

    // Indicates if the menu item is visible.
    isVisible?: BooleanInput;
}

export interface MenuItemNormalized extends MenuItemBase {
    isDisabled: Observable<boolean>;
    isVisible: Observable<boolean>;
}

@Component({
    selector: 'sqx-menu',
    styleUrls: ['./menu.component.scss'],
    templateUrl: './menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ConfirmClickDirective,
        DropdownMenuComponent,
        ModalDirective,
        ModalPlacementDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class MenuComponent implements AfterViewInit, ResizeListener {
    private readonly subscriptions = new Subscriptions();
    private measuredContainer?: number;
    private measuredCustom?: number;
    private measuredMenu?: number;

    @Input({ required: true })
    public set items(values: ReadonlyArray<MenuItem> ) {
        const normalize = (source: BooleanInput, defaultValue: boolean) => {
            if (Types.isBoolean(source)) {
                return of(source);
            } else {
                return source || of(defaultValue);
            }
        };

        this.itemsNormalized = values.map(({ isDisabled, isVisible, ...other } ) => ({
            isVisible: normalize(isVisible, true),
            isDisabled: normalize(isDisabled, false),
            ...other,
        }));

        const dropdownItems = this.itemsNormalized.filter(x => !x.showAlways && (!!x.label || !!x.menuLabel));

        this.hasVisibleDropdownItems =
            combineLatest(dropdownItems.map(x => x.isVisible))
                .pipe(map(values => values.some(x => x)));

        this.hasOnlyDisabledDropdownItems =
            combineLatest(dropdownItems.map(x => x.isDisabled))
                .pipe(map(values => !values.some(x => !x)));

        this.hasVisibleAlwaysItems =
            combineLatest(this.itemsNormalized.filter(x => x.showAlways).map(x => x.isVisible))
                .pipe(map(values => values.some(x => x)));
    }

    @Input()
    public alignment: 'left' | 'right' = 'left';

    @Input({ transform: booleanAttribute })
    public small = false;

    @Input({ transform: booleanAttribute })
    public showCustom = true;

    @ViewChild('container', { static: true })
    public container!: ElementRef<HTMLDivElement>;

    @ViewChild('custom', { static: true })
    public custom!: ElementRef<HTMLDivElement>;

    @ViewChild('menu', { static: true })
    public menu!: ElementRef<HTMLDivElement>;

    public isOverlapping = signal(false);

    public itemsNormalized: ReadonlyArray<MenuItemNormalized> = [];
    public itemsDropdown = new ModalModel();

    public hasVisibleDropdownItems: Observable<boolean> = of(false);
    public hasVisibleAlwaysItems: Observable<boolean> = of(false);
    public hasOnlyDisabledDropdownItems: Observable<boolean> = of(false);

    public get isRightAligned() {
        return this.alignment === 'right';
    }

    constructor(
        private readonly resizeService: ResizeService,
    ) {
    }

    public ngAfterViewInit() {
        this.subscriptions.add(this.resizeService.listen(this.container.nativeElement, this));
        this.subscriptions.add(this.resizeService.listen(this.custom.nativeElement, this));
        this.subscriptions.add(this.resizeService.listen(this.menu.nativeElement, this));
    }

    public onResize(rect: DOMRect, element: Element): void {
        if (element === this.container.nativeElement) {
            this.measuredContainer = rect.width;
        } else if (element === this.custom.nativeElement) {
            this.measuredCustom = Math.max(rect.width, element.scrollWidth);
        } else {
            this.measuredMenu = Math.max(rect.width, element.scrollWidth);
        }

        if (!this.measuredContainer || !this.measuredMenu || !this.measuredCustom) {
            return;
        }

        const isOverlapping = (this.measuredMenu + this.measuredCustom) > this.measuredContainer;
        this.isOverlapping.set(isOverlapping);
    }
}