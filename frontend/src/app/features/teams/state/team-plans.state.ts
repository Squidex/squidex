/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { TeamPlansService } from '@app/features/teams/internal';
import { DialogService, LoadingState, PlanDto, PlanLockedReason, ReferralDto, shareSubscribed, State, TeamsState, Version } from '@app/shared';

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

    // The team version.
    version: Version;
}

@Injectable()
export class TeamPlansState extends State<Snapshot> {
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

    public get teamId() {
        return this.teamsState.teamId;
    }

    public window = window;

    constructor(
        private readonly teamsState: TeamsState,
        private readonly dialogs: DialogService,
        private readonly plansService: TeamPlansService,
    ) {
        super({ plans: [], version: Version.EMPTY }, 'Teams Plans');
    }

    public load(isReload = false, overridePlanId?: string): Observable<any> {
        if (!isReload) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload, overridePlanId);
    }

    private loadInternal(isReload: boolean, overridePlanId?: string): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        return this.plansService.getPlans(this.teamId).pipe(
            tap(({ version, payload }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:plans.reloaded');
                }

                const planId = overridePlanId || payload.currentPlanId;
                const plans = payload.plans.map(x => createPlan(x, planId));

                this.next({
                    isLoaded: true,
                    isLoading: false,
                    locked: payload.locked,
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
        return this.plansService.putPlan(this.teamId, { planId }, this.version).pipe(
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
