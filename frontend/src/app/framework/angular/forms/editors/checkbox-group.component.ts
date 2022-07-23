/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewChecked, AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, OnChanges, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { getTagValues, MathHelper, StatefulControlComponent, TagValue, Types } from '@app/framework/internal';

export const SQX_CHECKBOX_GROUP_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => CheckboxGroupComponent), multi: true,
};

let CACHED_FONT: string;

interface State {
    // The checked values.
    checkedValues: ReadonlyArray<TagValue>;

    // True when all checkboxes can be shown as single line.
    isSingleline?: boolean;
}

@Component({
    selector: 'sqx-checkbox-group',
    styleUrls: ['./checkbox-group.component.scss'],
    templateUrl: './checkbox-group.component.html',
    providers: [
        SQX_CHECKBOX_GROUP_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckboxGroupComponent extends StatefulControlComponent<State, string[]> implements AfterViewInit, AfterViewChecked, OnChanges {
    private childrenWidth = 0;
    private checkedValuesRaw: any;
    private containerWidth = 0;
    private labelsMeasured = false;

    public readonly controlId = MathHelper.guid();

    @ViewChild('container', { static: false })
    public containerElement!: ElementRef<HTMLDivElement>;

    @Input()
    public layout: 'Auto' | 'Singletine' | 'Multiline' = 'Auto';

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @Input()
    public set values(value: ReadonlyArray<string | TagValue>) {
        this.valuesSorted = getTagValues(value);

        this.writeValue(this.checkedValuesRaw);
    }

    public valuesSorted: ReadonlyArray<TagValue> = [];

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {
            checkedValues: [],
        });
    }

    public ngAfterViewInit() {
        this.calculateWidth();
    }

    public ngAfterViewChecked() {
        this.calculateWidth();
    }

    public ngOnChanges() {
        this.labelsMeasured = false;

        this.calculateWidth();
    }

    public updateContainerWidth(width: number) {
        this.containerWidth = width;

        this.calculateSingleLine();
    }

    private calculateWidth() {
        this.calculateStyle();

        if (this.labelsMeasured) {
            this.calculateSingleLine();
            return;
        }

        if (!CACHED_FONT ||
            !this.containerElement ||
            !this.containerElement.nativeElement) {
            return;
        }

        if (!canvas) {
            canvas = document.createElement('canvas');
        }

        if (!canvas) {
            return;
        }

        const ctx = canvas.getContext('2d');

        if (ctx) {
            ctx.font = CACHED_FONT;

            let width = 0;

            for (const value of this.valuesSorted) {
                width += 40;
                width += ctx.measureText(value.name).width;
            }

            this.childrenWidth = width;
            this.calculateSingleLine();

            this.labelsMeasured = true;
        }
    }

    private calculateSingleLine() {
        let isSingleline = false;

        if (this.layout !== 'Auto') {
            isSingleline = this.layout === 'Singletine';
        } else {
            isSingleline = this.childrenWidth < this.containerWidth;
        }

        this.next({ isSingleline });
    }

    private calculateStyle() {
        if (CACHED_FONT ||
            !this.containerElement ||
            !this.containerElement.nativeElement) {
            return;
        }

        const style = window.getComputedStyle(this.containerElement.nativeElement);

        const fontSize = style.getPropertyValue('font-size');
        const fontFamily = style.getPropertyValue('font-family');

        if (!fontSize || !fontFamily) {
            return;
        }

        CACHED_FONT = `${fontSize} ${fontFamily}`;
    }

    public writeValue(obj: any) {
        this.checkedValuesRaw = obj;

        let checkedValues: TagValue[] = [];

        if (Types.isArray(obj) && obj.length > 0) {
            checkedValues = this.valuesSorted.filter(x => obj.includes(x.value));
        }

        this.next({ checkedValues });
    }

    public check(isChecked: boolean, value: TagValue) {
        let checkedValues = this.snapshot.checkedValues;

        if (isChecked) {
            checkedValues = [value, ...checkedValues];
        } else {
            checkedValues = checkedValues.removed(value);
        }

        this.next({ checkedValues });

        this.callChange(checkedValues.map(x => x.id));
    }

    public isChecked(value: TagValue) {
        return this.snapshot.checkedValues.includes(value);
    }

    public trackByValue(_index: number, tag: TagValue) {
        return tag.id;
    }
}

let canvas: HTMLCanvasElement | null = null;
