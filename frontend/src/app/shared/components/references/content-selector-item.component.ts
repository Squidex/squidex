/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */

import { booleanAttribute, ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { StopClickDirective } from '@app/framework';
import { AppLanguageDto, ContentDto, META_FIELDS, SchemaDto } from '@app/shared/internal';
import { ContentListCellDirective } from '../contents/content-list-cell.directive';
import { ContentListFieldComponent } from '../contents/content-list-field.component';

@Component({
    selector: '[sqxContentSelectorItem][language][languages][schema]',
    styleUrls: ['./content-selector-item.component.scss'],
    templateUrl: './content-selector-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ContentListCellDirective,
        ContentListFieldComponent,
        StopClickDirective,
    ],
})
export class ContentSelectorItemComponent {
    /** Emits when the user selects or deselects the content. */
    public readonly selectedChange = output<boolean>();

    /** Marks the content as selected. */
    public readonly selected = input(false, { transform: booleanAttribute });

    /** Allows the user to select the content. */
    public readonly selectable = input(true, { transform: booleanAttribute });

    /** The language to render localized fields in. */
    public readonly language = input.required<AppLanguageDto>();

    /** The languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The schema of the content. */
    public readonly schema = input.required<SchemaDto>();

    /** The content to render. */
    public readonly content = input.required<ContentDto>({ alias: 'sqxContentSelectorItem' });

    protected readonly metaFields = META_FIELDS;

    protected toggle() {
        if (this.selectable()) {
            this.select(!this.selected());
        }
    }

    protected select(isSelected: boolean) {
        this.selectedChange.emit(isSelected);
    }
}
