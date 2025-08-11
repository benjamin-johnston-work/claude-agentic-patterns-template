import { Metadata } from 'next';
import { DashboardOverview } from '@/components/dashboard/DashboardOverview';

export const metadata: Metadata = {
  title: 'Dashboard - Archie',
  description: 'Repository analysis dashboard and overview',
};

export default function DashboardPage() {
  return <DashboardOverview />;
}