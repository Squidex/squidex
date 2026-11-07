
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable @angular-eslint/component-selector */


import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, ChangeDetectorRef, Component, computed, input, output, viewChildren } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { of } from 'rxjs';
import { distinctUntilChanged, map, startWith } from 'rxjs/operators';
import { AppLanguageDto, ConfirmClickDirective, ContentDto, ContentListCellDirective, ContentListCellResizeDirective, ContentListFieldComponent, ContentsState, ContentStatusComponent, DropdownMenuComponent, ExternalLinkDirective, ModalDirective, ModalModel, ModalPlacementDirective, PatchContentForm, SchemaDto, StopClickDirective, TableField, TableSettings, TabRouterlinkDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: '[sqxContent][language][languages][tableFields][schema][tableSettings]',
    styleUrls: ['./content.component.scss'],
    templateUrl: './content.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ConfirmClickDirective,
        ContentListCellDirective,
        ContentListCellResizeDirective,
        ContentListFieldComponent,
        ContentStatusComponent,
        DropdownMenuComponent,
        ExternalLinkDirective,
        FormsModule,
        ModalDirective,
        ModalPlacementDirective,
        RouterLink,
        StopClickDirective,
        TabRouterlinkDirective,
        TranslatePipe,
    ],
})
export class ContentComponent {
    private readonly fields = viewChildren(ContentListFieldComponent);

    /** Emits when the user clones the content. */
    public readonly clone = output();

    /** Emits when the user deletes the content. */
    public readonly delete = output();

    /** Emits the new status when the user changes the status of the content. */
    public readonly statusChange = output<string>();

    /** Emits when the user selects or deselects the content. */
    public readonly selectedChange = output<boolean>();

    /** Renders the content as selected. */
    public readonly selected = input(false, { transform: booleanAttribute });

    /** The language to render the values in. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** The schema of the content. */
    public readonly schema = input<SchemaDto>();

    /** The fields to render as columns. */
    public readonly tableFields = input.required<ReadonlyArray<TableField>>();

    /** The table settings, used to render nested values. */
    public readonly tableSettings = input.required<TableSettings>();

    /** Shows the action to clone the content. */
    public readonly cloneable = input(false, { transform: booleanAttribute });

    /** The router link to the content. */
    public readonly link = input<any>(null);

    /** The content to render. */
    public readonly content = input.required<ContentDto>({ alias: 'sqxContent' });

    public readonly dropdown = new ModalModel();

    public readonly patchAllowed = computed(() => {
        return this.content().canUpdate;
    });

    public readonly patchForm = computed(() => {
        if (!this.patchAllowed()) {
            return undefined;
        }

        return new PatchContentForm(this.tableFields(), this.language());
    });

    public readonly isDirty = computed(() => {
        const form = this.patchForm()?.form;
        if (!form) {
            return of(false);
        }

        return form.events.pipe(map(() => form.dirty), startWith(form.dirty), distinctUntilChanged());
    });

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly contentsState: ContentsState,
    ) {
    }

    public save() {
        const content = this.content();
        const patchForm = this.patchForm();
        if (!content.canUpdate || !patchForm) {
            return;
        }

        const value = patchForm.submit();
        if (!value) {
            return;
        }

        this.contentsState.patch(content, value)
            .subscribe({
                next: () => {
                    patchForm.submitCompleted({ noReset: true });

                    this.changeDetector.detectChanges();
                },
                error: error => {
                    patchForm.submitFailed(error);

                    this.changeDetector.detectChanges();
                },
            });
    }

    public shouldStop(field: TableField) {
        const isDirty = this.patchForm()?.form.dirty === true;

        if (field.rootField) {
            return isDirty || (field.rootField.isInlineEditable && this.patchAllowed());
        } else {
            return isDirty;
        }
    }

    public cancel() {
        this.patchForm()?.submitCompleted();

        for (const field of this.fields()) {
            field.reset();
        }
    }
}
