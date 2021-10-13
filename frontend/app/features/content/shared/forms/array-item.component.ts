/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, OnInit, Output, QueryList, SimpleChanges, ViewChildren } from '@angular/core';
import { AppLanguageDto, ComponentForm, EditContentForm, FieldDto, FieldFormatter, FieldSection, invalid$, ObjectForm, RootFieldDto, StatefulComponent, Types, value$ } from '@app/shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ComponentSectionComponent } from './component-section.component';

interface State {
    // The when the section is collapsed.
    isCollapsed: boolean;
}

@Component({
    selector: 'sqx-array-item[form][formContext][language][languages][index]',
    styleUrls: ['./array-item.component.scss'],
    templateUrl: './array-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArrayItemComponent extends StatefulComponent<State> implements OnChanges, OnInit {
    @Output()
    public itemRemove = new EventEmitter();

    @Output()
    public itemMove = new EventEmitter<number>();

    @Output()
    public clone = new EventEmitter();

    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public formModel: ObjectForm;

    @Input()
    public canUnset?: boolean | null;

    @Input()
    public isCollapsedInitial = false;

    @Input()
    public isFirst?: boolean | null;

    @Input()
    public isLast?: boolean | null;

    @Input()
    public isDisabled?: boolean | null;

    @Input()
    public index: number;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @ViewChildren(ComponentSectionComponent)
    public sections: QueryList<ComponentSectionComponent>;

    public isCollapsed = false;
    public isInvalid: Observable<boolean>;

    public title: Observable<string>;

    constructor(changeDetector: ChangeDetectorRef,
    ) {
        super(changeDetector, {
            isCollapsed: false,
        });
    }

    public ngOnInit() {
        this.next({ isCollapsed: this.isCollapsedInitial });
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['formModel']) {
            this.isInvalid = invalid$(this.formModel.form);

            this.title = value$(this.formModel.form).pipe(map(x => this.getTitle(x)));
        }
    }

    private getTitle(value: any) {
        const values: string[] = [];

        if (Types.is(this.formModel, ComponentForm) && this.formModel.schema) {
            values.push(this.formModel.schema.displayName);
        }

        if (Types.is(this.formModel.field, RootFieldDto)) {
            for (const field of this.formModel.field.nested) {
                const fieldValue = value[field.name];

                if (fieldValue) {
                    const formatted = FieldFormatter.format(field, fieldValue);

                    if (formatted) {
                        values.push(formatted);
                    }
                }
            }
        }

        return values.join(', ');
    }

    public collapse() {
        this.next({ isCollapsed: true });
    }

    public expand() {
        this.next({ isCollapsed: false });
    }

    public moveTop() {
        this.itemMove.emit(0);
    }

    public moveUp() {
        this.itemMove.emit(this.index - 1);
    }

    public moveDown() {
        this.itemMove.emit(this.index + 1);
    }

    public moveBottom() {
        this.itemMove.emit(99999);
    }

    public reset() {
        this.sections.forEach(section => {
            section.reset();
        });
    }

    public trackBySection(_index: number, section: FieldSection<FieldDto, any>) {
        return section.separator?.fieldId;
    }
}
