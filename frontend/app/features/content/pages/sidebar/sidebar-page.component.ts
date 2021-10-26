/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, ChangeDetectorRef } from '@angular/core';
import { defined } from '@app/framework/internal';
import { ContentsState } from '@app/shared';
import { combineLatest, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { NavigationEnd, Router } from '@angular/router';
import { StatefulComponent, SchemasState } from '@app/shared';

interface State {
    url: string,
    name: string;
}

@Component({
    selector: 'sqx-sidebar-page',
    styleUrls: ['./sidebar-page.component.scss'],
    templateUrl: './sidebar-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SidebarPageComponent extends StatefulComponent<State> {
    public navigationSubscription;
    constructor(
        changeDetector: ChangeDetectorRef,
        public readonly contentsState: ContentsState,
        public readonly schemasState: SchemasState,
        private readonly router: Router,
    ) {
        super(changeDetector, {
            url: '',
            name: ''
        });
        this.navigationSubscription = this.router.events.subscribe((e: any) => {
            if (e instanceof NavigationEnd) {
                const selectedExtensionData: Observable<object> = combineLatest([
                    this.schemasState.selectedSchema.pipe(defined()),
                    this.contentsState.selectedContent
                ]).pipe(map(([schema, content]) => {
                    return content ? // @ts-ignore
                        JSON.parse(localStorage.getItem('selectedContentSidebarData')) : // @ts-ignore
                        JSON.parse(localStorage.getItem('selectedContentsSidebarData'));;

                }));
                selectedExtensionData.subscribe((item: State) => {
                    if (item) {
                        this.next({
                            url: item.url,
                            name: item.name
                        });
                    }
                })
            }
        });
    }

    public ngOnDestroy() {
        if (this.navigationSubscription) {
            this.navigationSubscription.unsubscribe();
        }
    }
}