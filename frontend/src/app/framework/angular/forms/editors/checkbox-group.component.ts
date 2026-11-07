/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, ElementRef, input, linkedSignal, model, signal, viewChild } from '@angular/core';
import { FormValueControl } from '@angular/forms/signals';
import { getTagValues, MathHelper, TagValue, TextMeasurer, Types } from '@app/framework/internal';
import { ResizedDirective } from '../../resized.directive';

@Component({
    selector: 'sqx-checkbox-group',
    styleUrls: ['./checkbox-group.component.scss'],
    templateUrl: './checkbox-group.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ResizedDirective,
    ],
})
export class CheckboxGroupComponent implements FormValueControl<ReadonlyArray<string> | null> {
    private readonly containerElement = viewChild<ElementRef<HTMLDivElement>>('container');
    private readonly containerWidth = signal(0);
    private readonly textMeasurer = new TextMeasurer(() => this.containerElement());

    private readonly tagValuesUnsorted = computed(() => {
        return getTagValues(this.values(), false);
    });

    private readonly tagValuesSorted = computed(() => {
        return this.tagValuesUnsorted().sortedByString(x => x.lowerCaseName);
    });

    private readonly tagValuesWidth = computed(() => {
        let width = 0;
        for (const value of this.tagValuesUnsorted()) {
            width += 40;
            width += this.textMeasurer.getTextSize(value.name);
        }

        return width;
    });

    protected readonly controlId = MathHelper.guid();

    /** The values of the checked options. */
    public readonly value = model<ReadonlyArray<string> | null>(null);

    /** Disables the control. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Indicates whether the control has been touched. */
    public readonly touched = model(false);

    /** Renders the options in a single line, in multiple lines or depending on the available space. */
    public readonly layout = input<'Auto' | 'Singleline' | 'Multiline'>('Auto');

    /** Keeps the order of the values instead of sorting them by name. */
    public readonly unsorted = input(true, { transform: booleanAttribute });

    /** The values to choose from. */
    public readonly values = input<ReadonlyArray<string | TagValue>>([]);

    protected readonly tagValues = computed(() => {
        return this.unsorted() ? this.tagValuesUnsorted() : this.tagValuesSorted();
    });

    protected readonly isSingleline = computed(() => {
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

    protected readonly checkedValues = linkedSignal<{ raw: unknown; tags: ReadonlyArray<TagValue> }, ReadonlyArray<TagValue>>({
        source: () => ({ raw: this.value(), tags: this.tagValuesUnsorted() }),
        computation: ({ raw, tags }, previous) => {
            // Keep the order of the user's selection, when the value was set by this control.
            if (previous && previous.source.tags === tags && Types.equals(previous.value.map(x => x.id), raw)) {
                return previous.value;
            }

            if (!Types.isArray(raw) || raw.length === 0) {
                return [];
            }

            return tags.filter(x => raw.includes(x.value));
        },
    });

    protected updateContainerWidth(width: number) {
        this.containerWidth.set(width);
    }

    protected check(isChecked: boolean, value: TagValue) {
        let checkedValues = this.checkedValues();

        if (isChecked) {
            checkedValues = [value, ...checkedValues];
        } else {
            checkedValues = checkedValues.removed(value);
        }

        this.checkedValues.set(checkedValues);
        this.value.set(checkedValues.map(x => x.id));
    }
}
