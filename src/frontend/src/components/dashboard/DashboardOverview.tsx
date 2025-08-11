'use client';

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { 
  FolderOpen, 
  Search, 
  BookOpen, 
  MessageSquare,
  Plus,
  TrendingUp,
  Clock,
  CheckCircle,
  AlertCircle
} from 'lucide-react';
import Link from 'next/link';

export function DashboardOverview() {
  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
          <p className="text-muted-foreground">
            Welcome back! Here's an overview of your repository analysis.
          </p>
        </div>
        <div className="flex items-center space-x-3">
          <Button asChild>
            <Link href="/repositories/add">
              <Plus className="w-4 h-4 mr-2" />
              Add Repository
            </Link>
          </Button>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Total Repositories
            </CardTitle>
            <FolderOpen className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">12</div>
            <p className="text-xs text-muted-foreground">
              +2 from last month
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Documentation Generated
            </CardTitle>
            <BookOpen className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">8</div>
            <p className="text-xs text-muted-foreground">
              66% completion rate
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Search Queries
            </CardTitle>
            <Search className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">143</div>
            <p className="text-xs text-muted-foreground">
              +12% from yesterday
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Active Conversations
            </CardTitle>
            <MessageSquare className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">5</div>
            <p className="text-xs text-muted-foreground">
              3 in progress
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Recent Activity and Quick Actions */}
      <div className="grid gap-6 lg:grid-cols-2">
        {/* Recent Repositories */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center">
              <FolderOpen className="w-5 h-5 mr-2" />
              Recent Repositories
            </CardTitle>
            <CardDescription>
              Recently connected and analyzed repositories
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {[
                {
                  name: 'archie-frontend',
                  language: 'TypeScript',
                  status: 'ready',
                  lastAnalyzed: '2 hours ago',
                },
                {
                  name: 'api-gateway',
                  language: 'C#',
                  status: 'analyzing',
                  lastAnalyzed: '1 day ago',
                },
                {
                  name: 'mobile-app',
                  language: 'React Native',
                  status: 'ready',
                  lastAnalyzed: '3 days ago',
                },
              ].map((repo) => (
                <div key={repo.name} className="flex items-center justify-between p-3 border rounded-lg">
                  <div className="flex items-center space-x-3">
                    <div className="w-2 h-2 rounded-full bg-green-500"></div>
                    <div>
                      <p className="text-sm font-medium">{repo.name}</p>
                      <p className="text-xs text-muted-foreground">{repo.language}</p>
                    </div>
                  </div>
                  <div className="flex items-center space-x-2">
                    <Badge 
                      variant={repo.status === 'ready' ? 'default' : 'secondary'}
                      className="text-xs"
                    >
                      {repo.status === 'ready' ? (
                        <CheckCircle className="w-3 h-3 mr-1" />
                      ) : (
                        <Clock className="w-3 h-3 mr-1" />
                      )}
                      {repo.status}
                    </Badge>
                    <span className="text-xs text-muted-foreground">{repo.lastAnalyzed}</span>
                  </div>
                </div>
              ))}
            </div>
            <div className="mt-4">
              <Button asChild variant="outline" className="w-full">
                <Link href="/repositories">View All Repositories</Link>
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Quick Actions */}
        <Card>
          <CardHeader>
            <CardTitle>Quick Actions</CardTitle>
            <CardDescription>
              Common tasks and shortcuts
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="grid gap-3">
              <Button asChild variant="outline" className="justify-start h-auto p-4">
                <Link href="/repositories/add">
                  <div className="flex items-start space-x-3">
                    <Plus className="w-5 h-5 mt-0.5" />
                    <div className="text-left">
                      <div className="font-medium">Connect New Repository</div>
                      <div className="text-sm text-muted-foreground">
                        Add a repository for analysis
                      </div>
                    </div>
                  </div>
                </Link>
              </Button>

              <Button asChild variant="outline" className="justify-start h-auto p-4">
                <Link href="/search">
                  <div className="flex items-start space-x-3">
                    <Search className="w-5 h-5 mt-0.5" />
                    <div className="text-left">
                      <div className="font-medium">Search Repositories</div>
                      <div className="text-sm text-muted-foreground">
                        Find code, documentation, and patterns
                      </div>
                    </div>
                  </div>
                </Link>
              </Button>

              <Button asChild variant="outline" className="justify-start h-auto p-4">
                <Link href="/chat">
                  <div className="flex items-start space-x-3">
                    <MessageSquare className="w-5 h-5 mt-0.5" />
                    <div className="text-left">
                      <div className="font-medium">Start Conversation</div>
                      <div className="text-sm text-muted-foreground">
                        Ask questions about your codebase
                      </div>
                    </div>
                  </div>
                </Link>
              </Button>

              <Button asChild variant="outline" className="justify-start h-auto p-4">
                <Link href="/documentation">
                  <div className="flex items-start space-x-3">
                    <BookOpen className="w-5 h-5 mt-0.5" />
                    <div className="text-left">
                      <div className="font-medium">View Documentation</div>
                      <div className="text-sm text-muted-foreground">
                        Browse AI-generated documentation
                      </div>
                    </div>
                  </div>
                </Link>
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* System Status */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center">
            <TrendingUp className="w-5 h-5 mr-2" />
            System Status
          </CardTitle>
          <CardDescription>
            Current system health and performance metrics
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-3">
            <div className="flex items-center space-x-3">
              <div className="w-3 h-3 bg-green-500 rounded-full"></div>
              <div>
                <p className="text-sm font-medium">GraphQL API</p>
                <p className="text-xs text-muted-foreground">Operational</p>
              </div>
            </div>
            <div className="flex items-center space-x-3">
              <div className="w-3 h-3 bg-green-500 rounded-full"></div>
              <div>
                <p className="text-sm font-medium">Search Service</p>
                <p className="text-xs text-muted-foreground">Operational</p>
              </div>
            </div>
            <div className="flex items-center space-x-3">
              <div className="w-3 h-3 bg-yellow-500 rounded-full"></div>
              <div>
                <p className="text-sm font-medium">AI Services</p>
                <p className="text-xs text-muted-foreground">High Load</p>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}