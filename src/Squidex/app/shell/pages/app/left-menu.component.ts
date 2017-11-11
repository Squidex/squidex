/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';

import { AppContext } from 'shared';

@Component({
    selector: 'sqx-left-menu',
    styleUrls: ['./left-menu.component.scss'],
    templateUrl: './left-menu.component.html',
    providers: [
        AppContext
    ]
})
export class LeftMenuComponent implements OnInit {
    public permission: string | null = null;

    constructor(public readonly ctx: AppContext
    ) {
    }

    public ngOnInit() {
        this.permission = this.ctx.app.permission;
    }
}