/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AfterViewChecked, AfterViewInit, booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, forwardRef, Input, ViewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { getTagValues, MathHelper, StatefulControlComponent, TagValue, TextMeasurer, Types } from '@app/framework/internal';
import { ResizedDirective } from '../../resized.directive';

export const SQX_CHECKBOX_GROUP_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => CheckboxGroupComponent), multi: true,
};

interface State {
    // The checked values.
    checkedValues: ReadonlyArray<TagValue>;

    // True when all checkboxes can be shown as single line.
    isSingleline?: boolean;
}

@Component({
    standalone: true,
    selector: 'sqx-checkbox-group',
    styleUrls: ['./checkbox-group.component.scss'],
    templateUrl: './checkbox-group.component.html',
    providers: [
        SQX_CHECKBOX_GROUP_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ResizedDirective,
    ],
})
export class CheckboxGroupComponent extends StatefulControlComponent<State, string[]> implements AfterViewInit, AfterViewChecked {
    private readonly textMeasurer: TextMeasurer;
    private childrenWidth = 0;
    private checkedValuesRaw: any;
    private containerWidth = 0;
    private labelsMeasured = false;

    public readonly controlId = MathHelper.guid();

    @ViewChild('container', { static: false })
    public containerElement!: ElementRef<HTMLDivElement>;

    @Input()
    public layout: 'Auto' | 'Singleline' | 'Multiline' = 'Auto';

    @Input({ transform: booleanAttribute })
    public unsorted = true;

    @Input({ transform: booleanAttribute })
    public set disabled(value: boolean | undefined | null) {
        this.setDisabledState(value === true);
    }

    @Input()
    public set values(value: ReadonlyArray<string | TagValue>) {
        this.tagValuesUnsorted = getTagValues(value, false);
        this.tagValuesSorted = this.tagValuesUnsorted.sortedByString(x => x.lowerCaseName);

        this.writeValue(this.checkedValuesRaw);
    }

    public get tagValues() {
        return !this.unsorted ? this.tagValuesSorted : this.tagValuesUnsorted;
    }

    public tagValuesSorted: ReadonlyArray<TagValue> = [];
    public tagValuesUnsorted: ReadonlyArray<TagValue> = [];

    constructor() {
        super({ checkedValues: [] });

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
        this.checkedValuesRaw = obj;

        let checkedValues: TagValue[] = [];

        if (Types.isArray(obj) && obj.length > 0) {
            checkedValues = this.tagValuesUnsorted.filter(x => obj.includes(x.value));
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
}
