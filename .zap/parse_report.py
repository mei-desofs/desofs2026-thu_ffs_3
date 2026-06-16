import json

r = json.load(open('.zap/zap-full-auth-report.json'))
site = r.get('site', [{}])[0]
alerts = site.get('alerts', [])
print('Total alertas:', len(alerts))
all_urls = set()
for a in alerts:
    for inst in a.get('instances', []):
        all_urls.add(inst.get('uri',''))
print('\nURLs unicos que geraram alertas (%d):' % len(all_urls))
for u in sorted(all_urls):
    print(' ', u)
