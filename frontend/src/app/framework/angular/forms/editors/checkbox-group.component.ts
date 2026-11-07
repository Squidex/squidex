/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, ElementRef, forwardRef, input, linkedSignal, signal, viewChild } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { getTagValues, MathHelper, SignalControlComponent, TagValue, TextMeasurer, Types } from '@app/framework/internal';
import { ResizedDirective } from '../../resized.directive';

export const SQX_CHECKBOX_GROUP_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => CheckboxGroupComponent), multi: true,
};

@Component({
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
export class CheckboxGroupComponent extends SignalControlComponent<string[]> {
    private readonly containerElement = viewChild<ElementRef<HTMLDivElement>>('container');
    private readonly containerWidth = signal(0);
    private readonly textMeasurer = new TextMeasurer(() => this.containerElement());
    private readonly checkedValuesRaw = signal<any>(undefined);

    public readonly controlId = MathHelper.guid();

    /** Renders the options in a single line, in multiple lines or depending on the available space. */
    public readonly layout = input<'Auto' | 'Singleline' | 'Multiline'>('Auto');

    /** Keeps the order of the values instead of sorting them by name. */
    public readonly unsorted = input(true, { transform: booleanAttribute });

    /** The values to choose from. */
    public readonly values = input<ReadonlyArray<string | TagValue>>([]);

    public readonly tagValuesUnsorted = computed(() => {
        return getTagValues(this.values(), false);
    });

    public readonly tagValuesSorted = computed(() => {
        return this.tagValuesUnsorted().sortedByString(x => x.lowerCaseName);
    });

    public readonly tagValues = computed(() => {
        return this.unsorted() ? this.tagValuesUnsorted() : this.tagValuesSorted();
    });

    public readonly tagValuesWidth = computed(() => {
        let width = 0;
        for (const value of this.tagValuesUnsorted()) {
            width += 40;
            width += this.textMeasurer.getTextSize(value.name);
        }

        return width;
    });

    public readonly isSingleline = computed(() => {
        const layout = this.layout();
        if (layout !== 'Auto') {
            return layout === 'Singleline';
        }

        const width = this.tagValuesWidth();
        if (width < 0) {
            return false;
        }

        return width < this.containerWidth();
    });

    public readonly checkedValues = linkedSignal<ReadonlyArray<TagValue>>(() => {
        const raw = this.checkedValuesRaw();
        if (!Types.isArray(raw) || raw.length === 0) {
            return [];
        }

        return this.tagValuesUnsorted().filter(x => raw.includes(x.value));
    });

    public updateContainerWidth(width: number) {
        this.containerWidth.set(width);
    }

    public writeValue(obj: any) {
        this.checkedValuesRaw.set(obj);
    }

    public check(isChecked: boolean, value: TagValue) {
        let checkedValues = this.checkedValues();

        if (isChecked) {
            checkedValues = [value, ...checkedValues];
        } else {
            checkedValues = checkedValues.removed(value);
        }

        this.checkedValues.set(checkedValues);

        this.callChange(checkedValues.map(x => x.id));
    }
}