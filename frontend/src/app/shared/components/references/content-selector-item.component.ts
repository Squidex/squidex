/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
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
        FormsModule,
        StopClickDirective,
    ]
})
export class ContentSelectorItemComponent {
    public readonly metaFields = META_FIELDS;

    @Output()
    public selectedChange = new EventEmitter<boolean>();

    @Input({ transform: booleanAttribute })
    public selected?: boolean | null;

    @Input({ transform: booleanAttribute })
    public selectable?: boolean | null = true;

    @Input()
    public language!: AppLanguageDto;

    @Input()
    public languages!: ReadonlyArray<AppLanguageDto>;

    @Input()
    public schema!: SchemaDto;

    @Input('sqxContentSelectorItem')
    public content!: ContentDto;

    public toggle() {
        if (this.selectable) {
            this.select(!this.selected);
        }
    }

    public select(isSelected: boolean) {
        this.selectedChange.emit(isSelected);
    }
}
