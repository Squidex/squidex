/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, computed, ElementRef, input, model, signal, viewChild } from '@angular/core';
import { getTagValues, MathHelper, TagValue, TextMeasurer } from '@app/framework/internal';
import { ResizedDirective } from '../../resized.directive';
import { FormValueControlBase } from '../form-value-control';

@Component({
    selector: 'sqx-radio-group',
    styleUrls: ['./radio-group.component.scss'],
    templateUrl: './radio-group.component.html',
    imports: [
        ResizedDirective,
    ],
})
export class RadioGroupComponent extends FormValueControlBase<any> {
    private readonly textMeasurer = new TextMeasurer(() => this.containerElement());

    /** The value of the selected option. */
    public readonly value = model<any>(null);

    /** Renders the options in a single line, in multiple lines or depending on the available space. */
    public readonly layout = input<'Auto' | 'Singleline' | 'Multiline'>('Auto');

    /** Keeps the order of the values instead of sorting them by name. */
    public readonly unsorted = input(true, { transform: booleanAttribute });

    /** The values to choose from. */
    public readonly values = input<ReadonlyArray<string | TagValue>>([]);

    protected readonly containerElement = viewChild<ElementRef<HTMLDivElement>>('container');

    protected readonly containerWidth = signal(0);

    protected readonly tagValuesUnsorted = computed(() => {
        return getTagValues(this.values(), false);
    });

    protected readonly tagValuesSorted = computed(() => {
        return this.tagValuesUnsorted().sortedByString(x => x.lowerCaseName);
    });

    protected readonly tagValuesWidth = computed(() => {
        let width = 0;
        for (const value of this.tagValuesUnsorted()) {
            width += 40;
            width += this.textMeasurer.getTextSize(value.name);
        }

        return width;
    });

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

    protected readonly controlId = MathHelper.guid();

    protected updateContainerWidth(width: number) {
        this.containerWidth.set(width);
    }

    protected select(value: TagValue) {
        if (this.disabled()) {
            return;
        }

        this.value.set(value.value);
    }
}
