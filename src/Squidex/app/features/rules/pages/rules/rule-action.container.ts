/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ComponentFactoryResolver, ComponentRef, Input, OnInit, ViewChild, ViewContainerRef } from '@angular/core';
import { FormGroup } from '@angular/forms';

export class RuleActionContainer implements OnInit {
    @Input()
    public actionType: string;

    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    @ViewChild('container', { read: ViewContainerRef })
    public entry: ViewContainerRef;

    private component: ComponentRef<any>;

    constructor(
        private readonly componentFactoryResolver: ComponentFactoryResolver
    ) {
    }

    public ngOnInit() {
        const factories = Array.from(this.componentFactoryResolver['_factories'].values());
        const factory: any = factories.find((x: any) => x.selector === this.actionType);

        this.component = this.entry.createComponent(factory);
    }
}