/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AfterViewChecked, AfterViewInit, ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, forwardRef, Input, OnChanges, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { getTagValues, MathHelper, StatefulControlComponent, TagValue, TextMeasurer } from '@app/framework/internal';

export const SQX_RADIO_GROUP_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => RadioGroupComponent), multi: true,
};

interface State {
    // True when all checkboxes can be shown as single line.
    isSingleline?: boolean;
}

@Component({
    selector: 'sqx-radio-group',
    styleUrls: ['./radio-group.component.scss'],
    templateUrl: './radio-group.component.html',
    providers: [
        SQX_RADIO_GROUP_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RadioGroupComponent extends StatefulControlComponent<State, string> implements AfterViewInit, AfterViewChecked, OnChanges {
    private readonly textMeasurer: TextMeasurer;
    private childrenWidth = 0;
    private containerWidth = 0;
    private labelsMeasured = false;

    public readonly controlId = MathHelper.guid();

    @ViewChild('container', { static: false })
    public containerElement!: ElementRef<HTMLDivElement>;

    @Input()
    public layout: 'Auto' | 'Singleline' | 'Multiline' = 'Auto';

    @Input()
    public unsorted = true;

    @Input()
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @Input()
    public set values(value: ReadonlyArray<string | TagValue>) {
        this.tagValuesUnsorted = getTagValues(value, false);
        this.tagValuesSorted = this.tagValuesUnsorted.sortedByString(x => x.lowerCaseName);
    }

    public get tagValues() {
        return !this.unsorted ? this.tagValuesSorted : this.tagValuesUnsorted;
    }

    public tagValuesSorted: ReadonlyArray<TagValue> = [];
    public tagValuesUnsorted: ReadonlyArray<TagValue> = [];

    public valueModel: any;

    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {});

        this.textMeasurer = new TextMeasurer(() => this.containerElement);
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
        if (this.labelsMeasured) {
            this.calculateSingleLine();
            return;
        }

        let width = 0;

        for (const value of this.tagValuesUnsorted) {
            width += 40;
            width += this.textMeasurer.getTextSize(value.name);
        }

        if (width < 0) {
            return;
        }

        this.childrenWidth = width;
        this.calculateSingleLine();

        this.labelsMeasured = true;
    }

    private calculateSingleLine() {
        let isSingleline = false;

        if (this.layout !== 'Auto') {
            isSingleline = this.layout === 'Singleline';
        } else {
            isSingleline = this.childrenWidth < this.containerWidth;
        }

        this.next({ isSingleline });
    }

    public writeValue(obj: any) {
        this.valueModel = obj;
    }

    public trackByValue(_index: number, tag: TagValue) {
        return tag.id;
    }
}
