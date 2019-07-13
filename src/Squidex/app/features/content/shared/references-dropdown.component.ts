/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Input, OnInit } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';
import { throwError } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import {
    AppLanguageDto,
    AppsState,
    ContentDto,
    ContentsService,
    FieldFormatter,
    fieldInvariant,
    ImmutableArray,
    MathHelper,
    RootFieldDto,
    SchemaDetailsDto,
    SchemasService,
    StatefulControlComponent,
    Types
} from '@app/shared';

export const SQX_REFERENCES_DROPDOWN_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ReferencesDropdownComponent), multi: true
};

interface State {
    schema?: SchemaDetailsDto | null;

    contentItems: ImmutableArray<ContentDto>;
    contentNames: ImmutableArray<ContentName>;
}

type ContentName = { name: string, id: string };

@Component({
    selector: 'sqx-references-dropdown',
    template: `
        <select class="form-control" [formControl]="selectedId">
            <option [ngValue]="null"></option>
            <option *ngFor="let content of snapshot.contentNames" [ngValue]="content.id">{{content.name}}</option>
        </select>`,
    providers: [SQX_REFERENCES_DROPDOWN_CONTROL_VALUE_ACCESSOR],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReferencesDropdownComponent extends StatefulControlComponent<State, string[]> implements OnInit {
    private languageField: AppLanguageDto;

    @Input()
    public schemaId: string;

    @Input()
    public set language(value: AppLanguageDto) {
        this.languageField = value;

        this.next(s => ({ ...s, contentNames: this.createContentNames(s.schema, s.contentItems) }));
    }

    public selectedId = new FormControl('');

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly schemasService: SchemasService
    ) {
        super(changeDetector, {
            schema: null,
            contentItems: ImmutableArray.empty(),
            contentNames: ImmutableArray.empty()
        });

        this.own(
            this.selectedId.valueChanges
                .subscribe(value => {
                    if (value) {
                        this.callTouched();
                        this.callChange([value]);
                    } else {
                        this.callTouched();
                        this.callChange([]);
                    }
                }));
    }

    public ngOnInit() {
        if (this.schemaId === MathHelper.EMPTY_GUID) {
            this.selectedId.disable();
            return;
        }

        this.schemasService.getSchema(this.appsState.appName, this.schemaId).pipe(
                switchMap(schema => {
                    if (schema) {
                        return this.contentsService.getContents(this.appsState.appName, this.schemaId, 100, 0);
                    } else {
                        return throwError('Invalid schema');
                    }
                }, (schema, contents) => ({ schema, contents })))
            .subscribe(({ schema, contents }) => {
                const contentItems = ImmutableArray.of(contents.items);
                const contentNames = this.createContentNames(schema, contentItems);

                this.next(s => ({ ...s, schema, contentItems, contentNames }));
            }, () => {
                this.selectedId.disable();
            });
    }

    public writeValue(obj: any) {
        if (Types.isArrayOfString(obj)) {
            this.selectedId.setValue(obj[0], { emitEvent: false });
        } else {
            this.selectedId.setValue(undefined, { emitEvent: false });
        }
    }

    private createContentNames(schema: SchemaDetailsDto | undefined | null, contents: ImmutableArray<ContentDto>): ImmutableArray<ContentName> {
        if (contents.length === 0 || !schema) {
            return ImmutableArray.empty();
        }

        function getRawValue(field: RootFieldDto, data: any, language: AppLanguageDto): any {
            const contentField = data[field.name];

            if (contentField) {
                if (field.isLocalizable) {
                    return contentField[language.iso2Code];
                } else {
                    return contentField[fieldInvariant];
                }
            }

            return undefined;
        }

        return contents.map(content => {
            const values: any[] = [];

            for (let field of schema.listFields) {
                const value = getRawValue(field, content.data, this.languageField);

                if (!Types.isUndefined(value)) {
                    values.push(FieldFormatter.format(field, value, false));
                }
            }

            const name = values.join(', ');

            return { name, id: content.id };
        });
    }

    public trackByContent(content: ContentDto) {
        return content.id;
    }
}