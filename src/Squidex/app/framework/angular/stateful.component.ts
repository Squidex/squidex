/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import { Types } from './../utils/types';

import { State } from '../state';

declare type UnsubscribeFunction = () => void;

export abstract class StatefulComponent<T> extends State<T> implements OnDestroy, OnInit {
    private subscriptions: (Subscription | UnsubscribeFunction)[] = [];

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        state: T
    ) {
        super(state);
    }

    protected observe(subscription: Subscription | UnsubscribeFunction) {
        if (subscription) {
            this.subscriptions.push(subscription);
        }
    }

    public ngOnInit() {
        this.changes.subscribe(() => {
            this.changeDetector.detectChanges();
        });
    }

    public ngOnDestroy() {
        try {
            for (let subscription of this.subscriptions) {
                if (Types.isFunction(subscription)) {
                    subscription();
                } else {
                    subscription.unsubscribe();
                }
            }
        } finally {
            this.subscriptions = [];
        }
    }
}

export abstract class PureComponent extends StatefulComponent<any> {
    constructor(changeDetector: ChangeDetectorRef) {
        super(changeDetector, {});
    }
}