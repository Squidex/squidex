/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, ComponentFactoryResolver, ComponentRef, Input, OnChanges, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { FormGroup } from '@angular/forms';

import actions from './actions';

@Component({
    selector: 'sqx-rule-action',
    template: '<div #element></div>',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RuleActionComponent implements OnChanges, OnInit {
    @Input()
    public actionType: string;

    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    @ViewChild('element', { read: ViewContainerRef })
    public viewContainer: ViewContainerRef;

    private component: ComponentRef<any>;

    constructor(
        private readonly componentFactoryResolver: ComponentFactoryResolver
    ) {
    }

    public ngOnChanges() {
        if (this.component) {
            this.component.instance.action = this.action;
            this.component.instance.actionForm = this.actionForm;
            this.component.instance.actionFormSubmitted = this.actionFormSubmitted;
        }
    }

    public ngOnInit() {
        const factoryType: any = actions[this.actionType];
        const factory: any = this.componentFactoryResolver.resolveComponentFactory(factoryType);

        this.component = this.viewContainer.createComponent(factory);

        this.component.instance.action = this.action;
        this.component.instance.actionForm = this.actionForm;
        this.component.instance.actionFormSubmitted = this.actionFormSubmitted;
    }
}