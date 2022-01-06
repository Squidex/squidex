/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, shareSubscribed, State, Version } from '@app/framework';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { AuthService } from './../services/auth.service';
import { PlanDto, PlansService } from './../services/plans.service';
import { AppsState } from './apps.state';

export interface PlanInfo {
    // The plan.
    plan: PlanDto;

    // Indicates if the yearly subscription is selected.
    isYearlySelected?: boolean;

    // Indicates if the monthly subscription is selected.
    isSelected?: boolean;
}

interface Snapshot {
    // The current plans.
    plans: ReadonlyArray<PlanInfo>;

    // Indicates if the user is the plan owner.
    isOwner?: boolean;

    // Indicates if the plans are loaded.
    isLoaded?: boolean;

    // Indicates if the plans are loading.
    isLoading?: boolean;

    // Indicates if there is a billing portal for the current Squidex instance.
    hasPortal?: boolean;

    // The app version.
    version: Version;
}

@Injectable()
export class PlansState extends State<Snapshot> {
    public plans =
        this.project(x => x.plans);

    public isOwner =
        this.project(x => x.isOwner === true);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public isDisabled =
        this.project(x => !x.isOwner);

    public hasPortal =
        this.project(x => x.hasPortal);

    public window = window;

    public get appId() {
        return this.appsState.appId;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly plansService: PlansService,
    ) {
        super({ plans: [], version: Version.EMPTY }, 'Plans');
    }

    public load(isReload = false, overridePlanId?: string): Observable<any> {
        if (!isReload) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload, overridePlanId);
    }

    private loadInternal(isReload: boolean, overridePlanId?: string): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        return this.plansService.getPlans(this.appName).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:plans.reloaded');
                }

                const planId = overridePlanId || payload.currentPlanId;
                const plans = payload.plans.map(x => createPlan(x, planId));

                this.next({
                    hasPortal: payload.hasPortal,
                    isLoaded: true,
                    isLoading: false,
                    isOwner: !payload.planOwner || payload.planOwner === this.userId,
                    plans,
                    version,
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs));
    }

    public change(planId: string): Observable<any> {
        return this.plansService.putPlan(this.appName, { planId }, this.version).pipe(
            tap(({ payload, version }) => {
                if (payload.redirectUri && payload.redirectUri.length > 0) {
                    this.window.location.href = payload.redirectUri;
                } else {
                    this.next(s => {
                        const plans = s.plans.map(x => createPlan(x.plan, planId));

                        return { ...s, plans, isOwner: true, version };
                    }, 'Change');
                }
            }),
            shareSubscribed(this.dialogs));
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get userId() {
        return this.authState.user!.id;
    }

    private get version() {
        return this.snapshot.version;
    }
}

function createPlan(plan: PlanDto, id: string) {
    return {
        plan,
        isSelected: plan.id === id,
        isYearlySelected: plan.yearlyId === id,
    };
}
