/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { DialogService, LoadingState, shareSubscribed, State, Version } from '@app/framework';
import { PlanDto, PlanLockedReason, PlansService, ReferralDto } from './../services/plans.service';
import { AppsState } from './apps.state';

export interface PlanInfo {
    // The plan.
    plan: PlanDto;

    // Indicates if the yearly subscription is selected.
    isYearlySelected?: boolean;

    // Indicates if the monthly subscription is selected.
    isSelected?: boolean;
}

interface Snapshot extends LoadingState {
    // The current plans.
    plans: ReadonlyArray<PlanInfo>;

    // The user, who owns the plan.
    planOwner?: string;

    // The portal link if available.
    portalLink?: string;

    // The referral info.
    referral?: ReferralDto;

    // The reason why the plan cannot be changed.
    locked?: PlanLockedReason;

    // The app version.
    version: Version;
}

@Injectable()
export class PlansState extends State<Snapshot> {
    public plans =
        this.project(x => x.plans);

    public planOwner =
        this.project(x => x.planOwner);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public locked =
        this.project(x => x.locked);

    public referral =
        this.project(x => x.referral);

    public portalLink =
        this.project(x => x.portalLink);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    public window = window;

    constructor(
        private readonly appsState: AppsState,
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
                    locked: payload.locked,
                    isLoaded: true,
                    isLoading: false,
                    planOwner: payload.planOwner,
                    plans,
                    portalLink: payload.portalLink,
                    referral: payload.referral,
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
