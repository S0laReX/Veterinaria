"""Pruebas HTTP con datos nuevos. Ejecutar SOLO contra una base de pruebas.
Uso: python scripts/verificar.py http://localhost:5198
Dependencias: requests, pypdf.
"""
import sys, uuid, io
from pathlib import Path
from datetime import datetime, timedelta
import requests
from html.parser import HTMLParser
class Element:
    def __init__(self, tag='', attrs=()):
        self.tag, self.attrs, self.children = tag, dict(attrs), []
    def __getitem__(self,key): return self.attrs[key]
    def get_text(self): return ''.join(c if isinstance(c,str) else c.get_text() for c in self.children)
    @property
    def text(self): return self.get_text()
    def select(self,selector):
        parts=selector.split()
        found=[]
        for c in self.children:
            if isinstance(c,Element):
                if c.tag==parts[0]: found.extend(c.select(' '.join(parts[1:])) if len(parts)>1 else [c])
                found.extend(c.select(selector))
        return found
    def find(self,tag,attrs=None,**kwargs):
        attrs={**(attrs or {}),**kwargs}
        return next((e for e in self.select(tag) if all(v(e.attrs.get(k)) if callable(v) else e.attrs.get(k)==v for k,v in attrs.items())),None)
class Parser(HTMLParser):
    def __init__(self,text):
        super().__init__(); self.root=Element();self.stack=[self.root];self.feed(text)
    def handle_starttag(self,tag,attrs):
        e=Element(tag,attrs);self.stack[-1].children.append(e)
        if tag not in ['input','meta','link','br','hr','img','source','wbr']: self.stack.append(e)
    def handle_endtag(self,tag):
        for i in range(len(self.stack)-1,0,-1):
            if self.stack[i].tag==tag: self.stack=self.stack[:i];break
    def handle_data(self,data): self.stack[-1].children.append(data)
def BeautifulSoup(text,parser): return Parser(text).root
from pypdf import PdfReader
BASE = sys.argv[1] if len(sys.argv)>1 else 'http://localhost:5198'
tag=uuid.uuid4().hex[:8]
passed=[]
def check(ok,name):
    assert ok,name
    passed.append(name)
    print('OK:', name)
def get(s,path):
    r=s.get(BASE+path); assert r.status_code==200,(path,r.status_code); return r

def post(s,path,data,token_path=None):
    r=get(s,token_path or path)
    t=BeautifulSoup(r.text,'html.parser').find('input',{'name':'__RequestVerificationToken'})
    assert t,('Falta antiforgery',path)
    return s.post(BASE+path,data={**data,'__RequestVerificationToken':t['value']},allow_redirects=False)

def login(s,email,password):
    r=post(s,'/Identity/Account/Login',{'Input.Email':email,'Input.Password':password})
    check(r.status_code==302,'Login '+email)

admin=requests.Session(); anon=requests.Session(); a=requests.Session(); b=requests.Session()
check('Nuestro cuidado' in get(anon,'/').text,'Landing pública')
for route in ['/Dashboard','/Reportes','/Servicios/Create','/Mascotas','/Citas']:
    r=anon.get(BASE+route,allow_redirects=False);check(r.status_code==302 and 'Login' in r.headers['Location'],'Anónimo bloqueado '+route)
login(admin,'admin@veterinaria.com','Admin123*')
service='Consulta '+tag
r=post(admin,'/Servicios/Create',{'Nombre':service,'Descripcion':'Atención veterinaria de prueba','Precio':'125.50'})
check(r.status_code==302,'Crear servicio')
html=get(admin,'/Servicios').text
soup=BeautifulSoup(html,'html.parser')
row=next(tr for tr in soup.select('tr') if service in tr.get_text())
sid=row.find('a',href=lambda h:h and '/Servicios/Edit/' in h)['href'].split('/')[-1]
r=post(admin,'/Servicios/Edit/'+sid,{'Id':sid,'Nombre':service,'Descripcion':'Consulta integral de prueba','Precio':'130.50'})
check(r.status_code==302,'Editar servicio')
check(service in get(anon,'/Home/Servicios').text,'Catálogo público conectado')
for session,name in [(a,'Cliente Uno'),(b,'Cliente Dos')]:
    email=name.replace(' ','').lower()+tag+'@example.test'
    r=post(session,'/Identity/Account/Register',{'Input.NombreCompleto':name,'Input.Email':email,'Input.Password':'Prueba123*','Input.ConfirmPassword':'Prueba123*','Role':'Administrador'})
    check(r.status_code==302,'Registro cliente '+name)
    check(name in get(session,'/').text,'Nombre completo '+name)
    check(get(session,'/Mascotas').status_code==200,'Rol Cliente '+name)
    for route in ['/Dashboard','/Reportes','/Servicios/Create','/Servicios']:
        r=session.get(BASE+route,allow_redirects=False);check(r.status_code==302 and 'AccessDenied' in r.headers['Location'],'Cliente bloqueado '+route)
pet='Luna '+tag
fields={'Nombre':pet,'Especie':'Gato','Raza':'Mestizo','Edad':'2','Sexo':'Hembra','Observaciones':'Prueba','UsuarioId':'atacante','Id':'888888'}
r=post(a,'/Mascotas/Create',fields);check(r.status_code==302,'Crear mascota sin confiar en propietario o Id enviados')
html=get(a,'/Mascotas').text
row=next(tr for tr in BeautifulSoup(html,'html.parser').select('tr') if pet in tr.get_text())
pid=row.find('a',href=lambda h:h and '/Mascotas/Edit/' in h)['href'].split('/')[-1]
check(pid!='888888','Id de mascota generado por base de datos')
check(pet not in get(b,'/Mascotas').text,'Mascotas aisladas por cliente')
check(b.get(BASE+'/Mascotas/Edit/'+pid).status_code==404,'No editar mascota ajena')
r=post(a,'/Mascotas/Edit/'+pid,{**fields,'Id':pid,'Edad':'3'});check(r.status_code==302,'Editar mascota propia')
r=post(a,'/Mascotas/Create',{**fields,'Edad':'-1'});check(r.status_code==200 and 'entre 0 y 100' in r.text,'Validación rango mascota')
# El segundo cliente necesita mascota propia para que Create muestre el formulario.
r=post(b,'/Mascotas/Create',{**fields,'Nombre':'Sol '+tag});check(r.status_code==302,'Mascota segundo cliente')
future=(datetime.now()+timedelta(days=2)).strftime('%Y-%m-%dT%H:%M')
appointment={'MascotaId':pid,'ServicioVeterinarioId':sid,'FechaCita':future,'Estado':'Atendida','UsuarioId':'atacante','Id':'888888'}
r=post(b,'/Citas/Create',appointment);check(r.status_code==200 and 'no pertenece' in r.text,'Rechazar cita con mascota ajena')
r=post(a,'/Citas/Create',{**appointment,'FechaCita':'2000-01-01T10:00'});check(r.status_code==200 and 'anterior' in r.text,'Rechazar cita pasada')
r=post(a,'/Citas/Create',{**appointment,'ServicioVeterinarioId':'9999999'});check(r.status_code==200 and 'no existe' in r.text,'Rechazar servicio inexistente')
r=post(a,'/Citas/Create',appointment);check(r.status_code==302,'Crear cita válida')
html=get(a,'/Citas').text;check(pet in html and 'Pendiente' in html,'Cita nace pendiente')
check(pet not in get(b,'/Citas').text,'Citas aisladas por cliente')
html=get(admin,'/Citas').text
row=next(tr for tr in BeautifulSoup(html,'html.parser').select('tr') if pet in tr.get_text())
cid=row.find('input',{'name':'id'})['value']
r=post(a,'/Citas/CambiarEstado',{'id':cid,'estado':'Atendida'},'/Citas');check(r.status_code==302 and 'AccessDenied' in r.headers['Location'],'Cliente no cambia estados')
r=post(admin,'/Citas/CambiarEstado',{'id':cid,'estado':'Atendida'},'/Citas');check(r.status_code==302 and 'Atendida' in get(a,'/Citas').text,'Administrador cambia estado')
r=post(admin,'/Citas/CambiarEstado',{'id':cid,'estado':'Inventado'},'/Citas');check(r.status_code==302 and 'Estado no' in get(admin,'/Citas').text,'Rechazar estado inválido')
r=admin.post(BASE+'/Citas/CambiarEstado',data={'id':cid,'estado':'Cancelada'});check(r.status_code==400,'Protección antiforgery')
r=post(a,'/Mascotas/Delete/'+pid,{},'/Mascotas');check(r.status_code==302 and pet in get(a,'/Mascotas').text,'Conservar mascota con citas')
r=post(admin,'/Servicios/Delete/'+sid,{});check(r.status_code==302 and service in get(admin,'/Servicios').text,'Conservar servicio con citas')
html=get(admin,'/Reportes').text
options=BeautifulSoup(html,'html.parser').select('select option')
uid=next(o['value'] for o in options if 'clienteuno'+tag in o.text)
check('admin@veterinaria.com' not in ''.join(o.text for o in options),'Selector reportes solo clientes')
out=Path('tmp/verificacion');out.mkdir(parents=True,exist_ok=True)
for path,needle,name in [('/Reportes/CitasGeneral',pet,'general'),('/Reportes/CitasPorCliente?usuarioId='+uid,pet,'cliente'),('/Reportes/ServiciosMasSolicitados',service,'servicios')]:
    r=get(admin,path);check(r.headers.get('Content-Type','').startswith('application/pdf'),'PDF '+name)
    text=''.join(p.extract_text() for p in PdfReader(io.BytesIO(r.content)).pages)
    check(needle in text,'PDF con datos reales '+name)
    (out/(name+'.pdf')).write_bytes(r.content)
# CRUD completo sobre registros propios sin citas.
r=post(admin,'/Servicios/Create',{'Nombre':'Eliminar '+tag,'Descripcion':'Temporal','Precio':'-1'})
check(r.status_code==200 and 'entre 0.01 y 10000' in r.text,'Rechazar precio negativo')
r=post(admin,'/Servicios/Create',{'Nombre':'Eliminar '+tag,'Descripcion':'Temporal','Precio':'20'})
check(r.status_code==302,'Crear servicio temporal')
row=next(tr for tr in BeautifulSoup(get(admin,'/Servicios').text,'html.parser').select('tr') if 'Eliminar '+tag in tr.text)
delete_id=row.find('a',href=lambda h:h and '/Servicios/Delete/' in h)['href'].split('/')[-1]
r=post(admin,'/Servicios/Delete/'+delete_id,{})
check(r.status_code==302 and 'Eliminar '+tag not in get(admin,'/Servicios').text,'Eliminar servicio sin citas')
row=next(tr for tr in BeautifulSoup(get(b,'/Mascotas').text,'html.parser').select('tr') if 'Sol '+tag in tr.text)
delete_pet=row.find('a',href=lambda h:h and '/Mascotas/Edit/' in h)['href'].split('/')[-1]
r=post(a,'/Mascotas/Delete/'+delete_pet,{},'/Mascotas')
check(r.status_code==404,'Rechazar eliminación de mascota ajena')
r=post(b,'/Mascotas/Delete/'+delete_pet,{},'/Mascotas')
check(r.status_code==302 and 'Sol '+tag not in get(b,'/Mascotas').text,'Eliminar mascota propia sin citas')
html=get(admin,'/Dashboard').text;check('chart-bars' in html and 'USUARIOS' in html,'Dashboard y gráfico sin CDN')
# Filtros combinados, ordenación, precio y persistencia de la búsqueda.
from urllib.parse import urlencode

def rows(session,path,params):
    response=get(session,path+'?'+urlencode(params))
    document=BeautifulSoup(response.text,'html.parser')
    return response, [r for r in document.select('tbody tr') if r.find('td')]

r,items=rows(admin,'/Servicios',{'Buscar':tag,'PrecioMin':'130','PrecioMax':'131'})
check(any(service in item.text for item in items),'Filtro combinado de servicio y precio')
r,items=rows(anon,'/Home/Servicios',{'Buscar':tag,'PrecioMax':'10'})
check(not any(service in item.text for item in items),'Catálogo excluye precios fuera de rango')
r,items=rows(anon,'/Home/Servicios',{'Buscar':tag,'PrecioMin':'200','PrecioMax':'100'})
check('no puede superar' in r.text,'Rango de precios invertido muestra validación')
r,items=rows(a,'/Mascotas',{'Buscar':tag,'Especie':'Gato','Sexo':'Hembra'})
check(any(pet in item.text for item in items),'Filtro combinado de mascotas')
r,items=rows(a,'/Mascotas',{'Buscar':tag,'Especie':'Perro'})
check(not any(pet in item.text for item in items),'Especie excluye resultados')
r,items=rows(b,'/Mascotas',{'Buscar':tag})
check(not any(pet in item.text for item in items),'Búsqueda respeta propietario de mascotas')
day=future[:10]
r,items=rows(a,'/Citas',{'Buscar':tag,'Estado':'Atendida','Desde':day,'Hasta':day})
check(any(pet in item.text for item in items),'Citas filtra estado y fechas inclusivas')
check(any('Bs' in item.text and ('130,50' in item.text or '130.50' in item.text) for item in items),'Citas muestran precio en bolivianos')
r,items=rows(a,'/Citas',{'Buscar':tag,'Estado':'Pendiente'})
check(not any(pet in item.text for item in items),'Estado excluye citas')
r,items=rows(b,'/Citas',{'Buscar':tag,'Estado':'Atendida'})
check(not any(pet in item.text for item in items),'Búsqueda respeta propietario de citas')
r,items=rows(a,'/Citas',{'Desde':'2026-12-31','Hasta':'2026-01-01'})
check('fecha inicial debe ser anterior' in r.text,'Rango de fechas invertido muestra validación')
r=get(a,'/Citas/Create?servicioId='+sid)
select=BeautifulSoup(r.text,'html.parser').find('select',{'name':'ServicioVeterinarioId'})
option=select.find('option',{'value':sid})
check('selected' in option.attrs and 'Bs' in option.text and 'data-precio' in option.attrs,'Servicio preseleccionado con precio al reservar')
back='/Citas?Estado=Atendida&Buscar='+tag
r=post(admin,'/Citas/CambiarEstado',{'id':cid,'estado':'Atendida','returnUrl':back},'/Citas')
check(r.status_code==302 and r.headers['Location']==back,'Cambiar estado conserva filtros')
r=post(admin,'/Citas/CambiarEstado',{'id':cid,'estado':'Atendida','returnUrl':'https://example.test'},'/Citas')
check(r.status_code==302 and r.headers['Location'].startswith('/Citas'),'Retorno rechaza redirección externa')
r=post(admin,'/Servicios/Create',{'Nombre':'Orden '+tag,'Descripcion':'Servicio de prueba','Precio':'25'})
check(r.status_code==302,'Crear segundo precio para verificar orden')
r,items=rows(admin,'/Servicios',{'Buscar':tag,'Orden':'precio-asc'})
check('Orden '+tag in items[0].text and service in items[-1].text,'Orden por menor precio')
r,items=rows(admin,'/Servicios',{'Buscar':tag,'Orden':'precio-desc'})
check(service in items[0].text and 'Orden '+tag in items[-1].text,'Orden por mayor precio')
r=get(admin,'/Reportes/CitasPorCliente?usuarioId='+uid)
pdf_text=''.join(p.extract_text() for p in PdfReader(io.BytesIO(r.content)).pages)
check('Precio actual (Bs)' in pdf_text and 'Bs' in pdf_text,'Reporte de citas incluye tarifa en Bs')
r=post(a,'/Identity/Account/Logout',{},'/');check(r.status_code==302,'Cerrar sesión')
check('Login' in a.get(BASE+'/Mascotas',allow_redirects=False).headers.get('Location',''),'Sesión cerrada')
print(f'\n{len(passed)} verificaciones correctas. Datos de prueba: {tag}')
(out/'resultado.txt').write_text('\n'.join(passed),encoding='utf-8')
